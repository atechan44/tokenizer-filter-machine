import sqlite3

conn = sqlite3.connect('words.db')
cursor = conn.cursor()

# Show tables
cursor.execute("SELECT name FROM sqlite_master WHERE type='table'")
print('Tables:', [r[0] for r in cursor.fetchall()])

# Show POSStatistics
cursor.execute('SELECT * FROM POSStatistics')
print('POSStatistics:')
for row in cursor.fetchall():
    print(f"  {row}")

conn.close()
