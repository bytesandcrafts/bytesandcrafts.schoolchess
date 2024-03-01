// See https://aka.ms/new-console-template for more information

using System.Text.RegularExpressions;

namespace bytesandcrafts.schoolchess;

static class SchoolChessTournamentManager
{
    static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Please specify the base folder path or --help to see the help content.");
            return;
        }
        
        if(args[0] == "--help")
        {
            Console.WriteLine(HelpContent.GetHelpContent());
            return;
        }

        var basePath = args[0];
        var rankingsPath = Path.Combine(basePath, "Rankings");
        var pairingsPath = Path.Combine(basePath, "Pairings");

        // Ensure directories exist
        Directory.CreateDirectory(rankingsPath);
        Directory.CreateDirectory(pairingsPath);

        var lastRankingFile = GetLatestFile(rankingsPath, "RankingsBeforeRound*");
        var lastPairingFile = GetLatestFile(pairingsPath, "PairingsRound*");

        if (lastRankingFile == null)
        {
            Console.WriteLine("No rankings file found. We have created a new rankings file for you. Please fill it in with players.");
            SaveRankings(new List<Player>(), Path.Combine(rankingsPath, "RankingsBeforeRound01.csv"));
            return;
        }

        var lastRanking = ReadRankings(lastRankingFile.FilePath!);
        var allPairings = ReadAllPairingsResults(pairingsPath);
        var currentRoundNumber = lastRankingFile.Round;
        var nextRoundNumber = lastPairingFile?.Round + 1 ?? 1;
        
        // Check if we have results in all pairings files
        foreach (var round in allPairings)
        {
            foreach (var pairing in round.Pairings)
            {
                var validResults = new [] { "1-0", "0-1", "0.5-0.5" };
                if (!validResults.Contains(pairing.Result))
                {
                    Console.WriteLine(
                        $"There is no result for {pairing.WhitePiecesPlayer.Name} vs {pairing.BlackPiecesPlayer.Name} in round {round.Number}. Please update the result in the pairings file. Valid results are: 1-0, 0-1, 0.5-0.5");
                    return;
                }
            }
        }
        
        // Generate pairings for the next round, if the file doesn't exist
        var newPairingFileName = Path.Combine(pairingsPath, $"PairingsRound{currentRoundNumber:D2}.csv");
        if (!File.Exists(newPairingFileName))
        {
            var newPairings = GeneratePairings(lastRanking, allPairings);

            SavePairings(newPairings, newPairingFileName);
            Console.WriteLine($"New pairings saved to {newPairingFileName}");
        }
        else 
        {
            Console.WriteLine($"Pairings file for round {currentRoundNumber} already exists. No new pairings generated.");
        }

        // Generate new rankings based on the latest pairings results, if the rankings file does not exist
        var newRankingFileName = Path.Combine(rankingsPath, $"RankingsBeforeRound{nextRoundNumber:D2}.csv");
        if (!File.Exists(newRankingFileName))
        {
            var newRankings = GenerateNewRankings(allPairings);
            
            SaveRankings(newRankings, newRankingFileName);
            Console.WriteLine($"New rankings saved to {newRankingFileName}");
        }
        else
        {
            Console.WriteLine($"Rankings file for round {nextRoundNumber} already exists. No new rankings generated.");
        }
    }

    static LatestFile? GetLatestFile(string path, string pattern)
    {
        var lastFileName = Directory.GetFiles(path, pattern).MaxBy(p => p);
        if (lastFileName != null)
        {
            return new LatestFile
            {
                FilePath = lastFileName,
                Round = int.Parse(Regex.Match(Path.GetFileNameWithoutExtension(lastFileName), @"\d+").Value)
            };
        }

        return null;
    }

    static List<Player> ReadRankings(string filePath)
    {
        var players = new List<Player>();
        var lines = File.ReadAllLines(filePath);
        foreach (var line in lines.Skip(1)) // Assuming the first line is header
        {
            var parts = line.Split(',');
            players.Add(new Player
            {
                Name = parts[0].Trim(),
                TotalPoints = string.IsNullOrEmpty(parts[1]) ? 0 : Convert.ToDecimal(parts[1])
            });
        }
        
        return players;
    }

    static List<Round> ReadAllPairingsResults(string pairingsFolderPath)
    {
        var allPairings = new List<Round>();
        var files = Directory.GetFiles(pairingsFolderPath, "PairingsRound*.csv");
        foreach (var file in files)
        {
            var round = new Round
            {
                Number = int.Parse(Regex.Match(Path.GetFileNameWithoutExtension(file), @"\d+").Value),
                Pairings = new List<Pairing>()
            };
            
            var lines = File.ReadAllLines(file);
            foreach (var line in lines.Skip(1)) // Assuming the first line is header
            {
                var parts = line.Split(',');
                round.Pairings.Add(new Pairing
                {
                    WhitePiecesPlayer = new Player { Name = parts[0].Trim() },
                    BlackPiecesPlayer = new Player { Name = parts[1].Trim() },
                    Result = parts[2].Trim()
                });
            }
            
            allPairings.Add(round);
        }

        return allPairings;
    }

    static void SaveRankings(List<Player> players, string filePath)
    {
        var lines = new List<string> { "Name, Total Points" };
        lines.AddRange(players.OrderByDescending(p=>p.TotalPoints).Select(p => $"{p.Name}, {p.TotalPoints}"));
        File.WriteAllLines(filePath, lines);
    }

    static void SavePairings(List<Pairing> pairings, string filePath)
    {
        var lines = new List<string> { "White Pieces Player, Black Pieces Player, Result" };
        lines.AddRange(pairings.Select(p => $"{p.WhitePiecesPlayer.Name}, {p.BlackPiecesPlayer.Name}, {p.Result}"));
        File.WriteAllLines(filePath, lines);
    }
    
    static List<Player> GenerateNewRankings(List<Round> allPairings)
    {
        // Pick up all results and generate new Rankings file
        var allPlayerNames = new Dictionary<string, decimal>();
        foreach (var round in allPairings)
        {
            foreach (var pairing in round.Pairings)
            {
                allPlayerNames.TryAdd(pairing.WhitePiecesPlayer.Name, 0);
                allPlayerNames.TryAdd(pairing.BlackPiecesPlayer.Name, 0);
                
                if (pairing.Result == "1-0")
                {
                    allPlayerNames[pairing.WhitePiecesPlayer.Name] += 1;
                }
                else if (pairing.Result == "0-1")
                {
                    allPlayerNames[pairing.BlackPiecesPlayer.Name] += 1;
                }
                else if (pairing.Result == "0.5-0.5")
                {
                    allPlayerNames[pairing.WhitePiecesPlayer.Name] += 0.5m;
                    allPlayerNames[pairing.BlackPiecesPlayer.Name] += 0.5m;
                }
            }
        }
        
        var players = allPlayerNames.Select(p => new Player { Name = p.Key, TotalPoints = p.Value }).ToList();
        return players;
    }

    static List<Pairing> GeneratePairings(List<Player> players, List<Round> allPreviousPairings)
    {
        // Simple pairing mechanism: pair adjacent players in the sorted list
        var sortedPlayers = players.OrderByDescending(p=>p.TotalPoints).ToList();
        var pairings = new List<Pairing>();
        for (int i = 0; i < sortedPlayers.Count; i += 2)
        {
            if (i + 1 < sortedPlayers.Count)
            {
                pairings.Add(new Pairing
                {
                    WhitePiecesPlayer = sortedPlayers[i],
                    BlackPiecesPlayer = sortedPlayers[i + 1],
                    Result = "" // Result is empty for new pairings
                });
            }
            else
            {
                // Handle bye
                sortedPlayers[i].TotalPoints += 1; // Award 1 point for a bye
            }
        }

        return pairings;
    }
}

class Player
{
    public required string Name { get; set; }
    public decimal TotalPoints { get; set; }
}

class Round
{
    public required int Number { get; set; }
    public required List<Pairing> Pairings { get; set; }
}

class Pairing
{
    public required Player WhitePiecesPlayer { get; set; }
    public required Player BlackPiecesPlayer { get; set; }
    public required string Result { get; set; } // Won, Lost, Draw
}

class LatestFile
{
    public string? FilePath { get; set; }
    public int Round  { get; set; }
}