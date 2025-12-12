import RPi.GPIO as GPIO
import time
import threading
import socket
import requests
import curses
from gpiozero import Motor, PWMOutputDevice
from datetime import datetime

# =============================
# KONFIGURATION
# =============================

AZURE_BASE_URL = "https://pishot-project-hqd6ffa0gvejbufu.canadacentral-01.azurewebsites.net/api"
UDP_PORT = 8080

# --- SERVO SETTINGS (STANDARD 180 GRADER) ---
SERVO_PIN = 16
PWM_FREQ = 50

# Standard rækkevidde for 180 graders servoer
# 2.0 - 2.5 = 0 grader (Helt i bund)
# 12.0 - 12.5 = 180 grader (Helt i top)
MIN_DC = 2.0
MAX_DC = 12.5
START_DC = 7.5  # Midten (90 grader)

# "Normal" hastighed.
# 0.4 er en god mellemting.
STEP = 0.4

# Skud indstillinger
FIRE_DC = 12.5
REST_DC = 7.5

# --- DC MOTOR SETTINGS ---
DC_FORWARD = 17
DC_BACKWARD = 22
DC_PWM_PIN = 18
DC_SPEED = 0.6  # Standard hastighed til motor

SHOT_WINDOW = 6.0

# =============================
# LOGGING
# =============================
def log_debug(msg):
    try:
        with open("pishot_log.txt", "a") as f:
            ts = datetime.now().strftime("%H:%M:%S")
            f.write(f"[{ts}] {msg}\n")
    except:
        pass

# =============================
# GLOBAL STATE
# =============================

state = {
    "shot_window_open": False,
    "current_shot_scored": False,
    "stop_flag": False,
    "is_uploading": False,
    "p1_id": 0,
    "p2_id": 0,
    "current_turn": "p1",
    "game_active": False
}

servo_pwm = None
current_servo_pos = START_DC
dc_motor = None
dc_pwm = None

# =============================
# HARDWARE INIT
# =============================

def init_hardware():
    global servo_pwm, dc_motor, dc_pwm, current_servo_pos

    # Servo
    GPIO.setwarnings(False)
    GPIO.setmode(GPIO.BOARD)
    GPIO.setup(SERVO_PIN, GPIO.OUT)

    servo_pwm = GPIO.PWM(SERVO_PIN, PWM_FREQ)
    servo_pwm.start(0)

    # Sæt start position
    current_servo_pos = START_DC
    servo_pwm.ChangeDutyCycle(current_servo_pos)
    time.sleep(0.2)

    # Motor
    dc_motor = Motor(forward=DC_FORWARD, backward=DC_BACKWARD)
    dc_pwm = PWMOutputDevice(DC_PWM_PIN)
    dc_pwm.value = 0

    log_debug("Hardware Initialized (Standard 180 Mode)")

def set_servo(dc):
    """Opdaterer servoen inden for normal 0-180 grader rækkevidde."""
    global current_servo_pos

    # Hold os inden for grænserne
    if dc < MIN_DC: dc = MIN_DC
    if dc > MAX_DC: dc = MAX_DC

    current_servo_pos = dc
    servo_pwm.ChangeDutyCycle(current_servo_pos)

def fire_catapult():
    """Skud funktion."""
    aim_pos = current_servo_pos

    # Skyd
    servo_pwm.ChangeDutyCycle(FIRE_DC)
    time.sleep(0.25)

    # Tilbage
    servo_pwm.ChangeDutyCycle(aim_pos)
    time.sleep(0.1)

# =============================
# API & GAME LOGIC
# =============================

def wait_for_uploads():
    timeout = 0
    while state["is_uploading"] and timeout < 50:
        time.sleep(0.1)
        timeout += 1

def wait_for_game_start(screen):
    screen.addstr(5, 0, "Status: WAITING FOR LOBBY...    ")
    screen.refresh()

    if dc_motor: dc_motor.stop()
    if dc_pwm: dc_pwm.value = 0

    while not state["stop_flag"]:
        try:
            r = requests.get(f"{AZURE_BASE_URL}/game/current", timeout=2)
            if r.status_code == 200:
                d = r.json()
                is_active = d.get("isActive") or d.get("IsActive")

                if is_active:
                    state["p1_id"] = d.get("player1Id") or d.get("Player1Id")
                    state["p2_id"] = d.get("player2Id") or d.get("Player2Id")
                    state["current_turn"] = "p1"
                    state["game_active"] = True
                    return
        except:
            pass
        time.sleep(1)

def check_round_winner(screen):
    screen.addstr(9, 0, "Checking Scores...              ")
    screen.refresh()
    wait_for_uploads()

    try:
        r = requests.get(f"{AZURE_BASE_URL}/scores/live", timeout=2)
        d = r.json()
        p1 = d.get("p1", {}).get("totalScore", 0)
        p2 = d.get("p2", {}).get("totalScore", 0)

        screen.addstr(5, 0, f"Status: Score {p1} - {p2}         ")

        winner_id = None
        if p1 >= 5 and p1 > p2: winner_id = state["p1_id"]
        elif p2 >= 5 and p2 > p1: winner_id = state["p2_id"]

        if winner_id:
            screen.addstr(9, 0, f"WINNER: {winner_id}!              ")
            requests.post(f"{AZURE_BASE_URL}/game/declare_winner", json={"winnerId": winner_id})
            return True
    except:
        pass
    return False

def udp_listener():
    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    sock.bind(("0.0.0.0", UDP_PORT))
    sock.settimeout(1.0)

    while not state["stop_flag"]:
        try:
            data, _ = sock.recvfrom(1024)
            if data == b"GOAL" and state["shot_window_open"]:
                state["current_shot_scored"] = True
                state["is_uploading"] = True
                pid = state["p1_id"] if state["current_turn"] == "p1" else state["p2_id"]
                try:
                    requests.post(f"{AZURE_BASE_URL}/scores", json={"profileId": pid}, timeout=3)
                except: pass
                finally: state["is_uploading"] = False
        except:
            continue
    sock.close()

def perform_shot(screen):
    dc_motor.stop()
    dc_pwm.value = 0

    pid = state["p1_id"] if state["current_turn"] == "p1" else state["p2_id"]
    screen.addstr(6, 0, f"SHOOTING: {pid}          ")
    state["current_shot_scored"] = False

    try:
        requests.post(f"{AZURE_BASE_URL}/scores/shot_attempt", json={"profileId": pid}, timeout=1)
    except: pass

    screen.refresh()
    fire_catapult()

    state["shot_window_open"] = True
    start = time.time()
    while time.time() - start < SHOT_WINDOW:
        rem = SHOT_WINDOW - (time.time() - start)
        screen.addstr(7, 0, f"Ball in Air: {rem:4.1f}s       ")
        screen.refresh()
        time.sleep(0.1)

    state["shot_window_open"] = False
    if state["is_uploading"]: wait_for_uploads()

    res = "GOAL!" if state["current_shot_scored"] else "MISS"
    screen.addstr(8, 0, f"RESULT: {res}                      ")
    screen.refresh()
    time.sleep(1)

    if state["current_turn"] == "p1":
        state["current_turn"] = "p2"
    else:
        if check_round_winner(screen):
            state["game_active"] = False
            time.sleep(3)
        else:
            state["current_turn"] = "p1"

# =============================
# MAIN CONTROL
# =============================

def main_control():
    screen = None
    threading.Thread(target=udp_listener, daemon=True).start()

    try:
        screen = curses.initscr()
        curses.noecho()
        curses.cbreak()
        screen.keypad(True)
        screen.nodelay(True)

        screen.addstr(0, 0, "--- PiShot Arena (NORMAL 180) ---")
        screen.addstr(1, 0, "LEFT/RIGHT: Aim (0-180)")
        screen.addstr(2, 0, "UP/DOWN: Move | ENTER: Shoot")

        while True:
            if not state["game_active"]:
                wait_for_game_start(screen)
                screen.clear()
                screen.addstr(0, 0, "--- PiShot Arena LIVE ---")

            p_txt = "P1" if state["current_turn"] == "p1" else "P2"
            screen.addstr(4, 0, f"TURN: {p_txt} | SERVO: {current_servo_pos:.1f}  ")

            key = screen.getch()

            if key == ord('q'):
                break

            # --- STYRING ---

            # SERVO (NORMAL TRINVIS)
            if key == curses.KEY_LEFT:
                set_servo(current_servo_pos - STEP)
            elif key == curses.KEY_RIGHT:
                set_servo(current_servo_pos + STEP)

            # DC MOTOR
            elif key == curses.KEY_UP:
                dc_motor.forward()
                dc_pwm.value = DC_SPEED
            elif key == curses.KEY_DOWN:
                dc_motor.backward()
                dc_pwm.value = DC_SPEED

            # SKYD
            elif key in (10, 13):
                perform_shot(screen)

            # STOP
            else:
                dc_motor.stop()
                dc_pwm.value = 0
                # Servo signal forbliver aktivt for at holde position

            screen.refresh()
            time.sleep(0.04)

    finally:
        state["stop_flag"] = True
        cleanup()
        if screen:
            curses.nocbreak()
            screen.keypad(False)
            curses.echo()
            curses.endwin()

def cleanup():
    try:
        if dc_motor: dc_motor.stop()
        if dc_pwm: dc_pwm.close()
        if servo_pwm: servo_pwm.stop()
    except: pass
    GPIO.cleanup()

if __name__ == "__main__":
    try:
        init_hardware()
        main_control()
    except KeyboardInterrupt:
        cleanup()
