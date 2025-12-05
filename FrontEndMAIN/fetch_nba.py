import json
from nba_api.stats.endpoints import leaguedashplayerstats

print("Fetching NBA 2024-25 Season Data...")

# Fetch all player stats for the current season
# "FG_PCT" is Field Goal Percentage (Accuracy)
stats = leaguedashplayerstats.LeagueDashPlayerStats(season='2024-25')
df = stats.get_data_frames()[0]

nba_players = []

for index, row in df.iterrows():
    # Only include players who have taken a decent number of shots 
    # (e.g., > 10 attempts) to avoid 100% accuracy outliers
    if row['FGA'] > 10:
        player_data = {
            "name": row['PLAYER_NAME'],
            "id": row['PLAYER_ID'],
            "accuracy": round(row['FG_PCT'] * 100, 1) # Convert 0.45 to 45.0
        }
        nba_players.append(player_data)

# Save to file
filename = "nba_2025.json"
with open(filename, "w") as f:
    json.dump(nba_players, f)

print(f"Done! Saved {len(nba_players)} players to {filename}")