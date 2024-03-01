namespace bytesandcrafts.schoolchess;

public class HelpContent
{
   public static string GetHelpContent() => @"
Hey There! 
Welcome to the School Chess Tournament Manager.

This is a simple command line utility that just reads the files in the specified folder and based on the rankings for a specific round, it generates the pairings for the next round.
The tournament starts with a folder structure like this:
- Tournament3rdGrade
   - Rankings
      - RankingsBeforeRound01.csv

RankingsBeforeRound01.txt file contains the following columns:
Name, Total Points

All fields are separated by a comma.

When the program runs, using the base folder path as the parameter, it will look for the last file in the Rankings folder and generate the pairings for that round. The pairing file will be saved in the Pairings folder.
- Tournament3rdGrade
   - Rankings
      - RankingsBeforeRound01.csv
   - Pairings
      - PairingsRound01.csv

User can then take the PairingsRound01.csv file and print it for the tournament. PairinigsRound01.csv file will contain the following columns:
White Pieces Player, Black Pieces Player, Result

School tournaments can be pretty messy...new players are added, removed, no shows, crying, etc. After that Round01 of madness, user can collect information and update PairingsRound01.csv file with the results and updated information (manually).

After that, user runs the program again. This time, it will look for the last file in the Pairings folder (which now contains populated Result column) and generate new Rankings and Pairings file. 
After this second run of the program, the folder structure will look something like this:
- Tournament3rdGrade
   - Rankings
      - RankingsBeforeRound01.csv
      - RankingsBeforeRound02.csv
   - Pairings
      - PairingsRound01.csv
      - PairingsRound02.csv

User can then take the PairingsRound02.csv file and print it for the tournament, run the tournament, collect the results and update those results in PairingsRound02.csv file.

Some rules when it comes to Rankings file:
- Rankings file will contain all the players that exist in the Pairings file for the round and all players that exist in the previous ranking file. (as some kids might show up in the second round or third round etc. and that information will be in the updated Pairings file).
- If a player is not in the Pairings file for the round, then the Total Points for that player will be the same as the previous round.
- If a player is in the Pairings file for the round, then the Total Points for that player will be the same as the previous round + 1 if the player won, + 0.5 if the player drew and + 0 if the player lost.

Some rules when it comes to create Pairings file:
- Pairings file will contain all the players that exist in the Rankings file for the round.
- Pairings file will contain the pairings for the round. The pairings will be generated based on the rankings for the round. The player will be matched with the player with the closest ranking to him/her.
- Player might not have a pair if there is an odd number of players in the round. In that case, the player will have a bye and will be awarded 1 point.
- Players are not allowed to play against the same player more than once in the tournament. The program uses previous Pairings files to make sure that the same player is not paired with the same player again.

This process can be repeated for as many rounds as needed.
";
}