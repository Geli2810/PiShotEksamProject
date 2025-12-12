import lgpio
import time
import socket

# ---------------------------
# KONFIG – GPIO / SENSOR
# ---------------------------
TRIG = 23          # TRIG på GPIO23
ECHO = 24          # ECHO på GPIO24
CHIP = 4           # Pi 5: chip 4

# Relativ ændring der skal til for at trigge mål
DIST_CHANGE_THRESHOLD = 0.10   # 10 %

# Cooldown mellem mål
COOLDOWN_TIME = 10.0           # 10 sekunder

# Gyldig afstand (NU: 6–11 cm)
MIN_VALID_DIST = 6.0           # cm (mindst 6 cm)
MAX_VALID_DIST = 11.0          # cm (maks 11 cm)

# ---------------------------
# KONFIG – UDP
# ---------------------------
CATAPULT_IP = "192.168.14.58"
UDP_PORT = 8080

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

print("=== Starter Goal Keeper (ændring i afstand = trigger) ===", flush=True)

# ---------------------------
# ÅBN GPIO-CHIP
# ---------------------------
try:
    print(f"Prøver at åbne gpiochip {CHIP}...", flush=True)
    h = lgpio.gpiochip_open(CHIP)
    print(f"Åbnede gpiochip {CHIP}", flush=True)
except Exception as e:
    print(f"Kunne ikke åbne chip {CHIP}: {e}", flush=True)
    print("Prøver chip 0 i stedet...", flush=True)
    h = lgpio.gpiochip_open(0)
    print("Åbnede gpiochip 0", flush=True)

# Claim pins
try:
    lgpio.gpio_claim_output(h, TRIG)
    lgpio.gpio_claim_input(h, ECHO)
    print(f"GPIO pins klar (TRIG={TRIG}, ECHO={ECHO})", flush=True)
except Exception as e:
    print(f"FEJL ved gpio_claim_*: {e}", flush=True)
    lgpio.gpiochip_close(h)
    sock.close()
    raise SystemExit("Kunne ikke claime GPIO – stop.")

# TRIG lav til at starte med
lgpio.gpio_write(h, TRIG, 0)
time.sleep(0.05)


def get_distance_cm():
    """Én måling fra ultralydssensoren, i cm eller None ved timeout."""
    # Send 10 µs pulse
    lgpio.gpio_write(h, TRIG, 1)
    time.sleep(0.00001)
    lgpio.gpio_write(h, TRIG, 0)

    # Vent på HIGH (start)
    timeout = time.time() + 0.1
    while lgpio.gpio_read(h, ECHO) == 0:
        if time.time() > timeout:
            return None

    start = time.time()

    # Vent på LOW (slut)
    timeout = time.time() + 0.1
    while lgpio.gpio_read(h, ECHO) == 1:
        if time.time() > timeout:
            return None

    end = time.time()

    duration = end - start
    distance_cm = (duration * 34300.0) / 2.0
    return distance_cm


def calibrate():
    """
    Laver nogle målinger uden bold, så I kan se hvad den normale afstand er.
    Kalibreringen viser bare hvad sensoren læser, men vores gyldige interval er 6–11 cm.
    """
    print("\nKalibrerer... sørg for at bolden IKKE er foran sensoren.", flush=True)
    samples = []
    target_samples = 10  # lidt færre så det går hurtigt

    while len(samples) < target_samples:
        d = get_distance_cm()
        if d is not None:
            print(f"Calib sample {len(samples)+1}/{target_samples}: {d:.1f} cm", flush=True)
            samples.append(d)
        time.sleep(0.05)

    baseline = sum(samples) / len(samples)
    print(f"\nKalibreret baseline: {baseline:.1f} cm", flush=True)

    if baseline < MIN_VALID_DIST or baseline > MAX_VALID_DIST:
        print(
            f"ADVARSEL: baseline {baseline:.1f} cm er udenfor ønsket interval "
            f"{MIN_VALID_DIST:.1f}–{MAX_VALID_DIST:.1f} cm.\n"
            "Tjek afstand og vinkler på sensoren.",
            flush=True
        )

    print(
        f"Sensor accepterer nu kun målinger mellem {MIN_VALID_DIST:.1f} cm og {MAX_VALID_DIST:.1f} cm.\n",
        flush=True
    )

    return baseline


baseline = calibrate()
print("Starter målinger... (tryk Ctrl+C for at stoppe)\n", flush=True)

try:
    prev_dist = None
    last_goal_time = 0.0
    last_print_time = 0.0
    last_cooldown_print = 0.0

    while True:
        now = time.time()
        dist = get_distance_cm()

        # COOLDOWN VISNING
        time_since_goal = now - last_goal_time
        if time_since_goal < COOLDOWN_TIME:
            remaining = COOLDOWN_TIME - time_since_goal
            # Print cooldown max én gang pr. 0.5 sekund
            if now - last_cooldown_print > 0.5:
                print(f"Cooldown: {remaining:4.1f} s tilbage før næste mål kan scores", flush=True)
                last_cooldown_print = now

        if dist is None:
            if now - last_print_time > 0.5:
                print("Ingen valid måling (timeout)", flush=True)
                last_print_time = now
            time.sleep(0.001)
            continue

        # Brug intervallet 6–11 cm
        if not (MIN_VALID_DIST <= dist <= MAX_VALID_DIST):
            if now - last_print_time > 0.5:
                print(f"Ugyldig måling (udenfor 6–11 cm): {dist:.1f} cm", flush=True)
                last_print_time = now
            time.sleep(0.001)
            continue

        # Debug-print ca. hver 0.2 s
        if now - last_print_time > 0.2:
            if prev_dist is None:
                print(f"Distance: {dist:.1f} cm (første måling)", flush=True)
            else:
                delta = dist - prev_dist
                rel_change_pct = (delta / prev_dist) * 100.0 if prev_dist != 0 else 0.0
                print(
                    f"Distance: {dist:.1f} cm | Δ: {delta:+.1f} cm ({rel_change_pct:+.1f}%)",
                    flush=True
                )
            last_print_time = now

        # Første måling: bare sæt prev_dist
        if prev_dist is None:
            prev_dist = dist
            time.sleep(0.001)
            continue

        # Beregn relativ ændring
        delta = abs(dist - prev_dist)
        rel_change = delta / prev_dist if prev_dist != 0 else 0.0

        ready_for_new_goal = time_since_goal >= COOLDOWN_TIME

        if ready_for_new_goal and rel_change >= DIST_CHANGE_THRESHOLD:
            print("\n>>> ⚽ GOAL! (stor ændring i afstand) ⚽ <<<", flush=True)
            print(f"Starter {COOLDOWN_TIME:.0f} sekunders cooldown...\n", flush=True)
            try:
                sock.sendto(b"GOAL", (CATAPULT_IP, UDP_PORT))
                print(f"UDP 'GOAL' sendt til {CATAPULT_IP}:{UDP_PORT}\n", flush=True)
            except Exception as e:
                print(f"FEJL ved UDP send: {e}", flush=True)

            last_goal_time = time.time()
            last_cooldown_print = last_goal_time

        # Opdater for næste iteration
        prev_dist = dist
        time.sleep(0.001)

except KeyboardInterrupt:
    print("\nStopper (Ctrl+C).", flush=True)

finally:
    try:
        lgpio.gpiochip_close(h)
        print("gpiochip lukket.", flush=True)
    except Exception:
        pass

    try:
        sock.close()
        print("UDP socket lukket.", flush=True)
    except Exception:
        pass

    print("Farvel :)", flush=True)
