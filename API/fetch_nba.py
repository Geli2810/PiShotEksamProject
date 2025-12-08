import json
import pandas as pd
from nba_api.stats.endpoints import leaguedashplayerstats
import time

def fetch_nba_data():
    print("--- Starter NBA Data Hentning (2024-25 Sæson) ---")
    
    try:
        # Hent spillere for den nuværende sæson
        # FG_PCT er Field Goal Percentage (Præcision)
        stats = leaguedashplayerstats.LeagueDashPlayerStats(season='2024-25')
        df = stats.get_data_frames()[0]
        
        nba_players = []

        print(f"Data hentet. Behandler {len(df)} spillere...")

        for index, row in df.iterrows():
            # Vi filtrerer spillere fra, der har taget meget få skud (f.eks. under 10)
            # for at undgå at nogen med 1 skud og 100% præcision ødelægger statistikken.
            if row['FGA'] > 10:
                player_data = {
                    "name": row['PLAYER_NAME'],
                    "id": row['PLAYER_ID'],
                    # Konverter 0.455 til 45.5
                    "accuracy": round(row['FG_PCT'] * 100, 1) 
                }
                nba_players.append(player_data)

        # Gem til JSON fil
        filename = "nba_2025.json"
        with open(filename, "w") as f:
            json.dump(nba_players, f)

        print(f"SUCCES! {len(nba_players)} spillere gemt i '{filename}'.")
        print("Du kan nu åbne index.html.")

    except Exception as e:
        print(f"FEJL: Kunne ikke hente data. Årsag: {e}")

if __name__ == "__main__":
    fetch_nba_data()