using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace NFLParsing
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            String[] data = File.ReadAllLines(@"C:\nflplays.csv");
            String line;
            

            for (long cursor = 0; cursor < data.LongLength; cursor++)
            {
                line = data[cursor];

                if (line.Contains("INTERCEPTION|TOUCHDOWN"))
                {
                    using (StreamWriter sw = File.AppendText(@"C:\results.csv"))
                    {
                        sw.WriteLine(data[cursor]);
                    }
                }
                else if (line.Contains("MUFFS"))
                {
                    // Receiving team touched the ball but fumbled.  Did the other team recover for a touchdown during that same play?
                    if (line.Contains("TOUCHDOWN"))
                    {
                        using (StreamWriter sw = File.AppendText(@"C:\results.csv"))
                        {
                            sw.WriteLine(data[cursor]);
                        }
                    }
                    else
                    {
                        // Scoring drives off a muff
                        cursor = CheckForScoringDrive(data, line, cursor);
                    }
                }
                else if (line.Contains("INTERCEPTION") || line.Contains("FUMBLE"))
                {
                    //FUMBLE|PUNT situations are tough, punting team or receiving team can recover.  Look for "RECOVERED by"?

                    cursor = CheckForScoringDrive(data, line, cursor);
                }
            }
        }

        private long CheckForScoringDrive(String[] data, String currentPlay, long cursor)
        {
            String[] data_columns;
            String game_id;
            String teamMakingInterception;
            String new_offense;
            String new_game_id;

            data_columns = currentPlay.Split('\t');
            game_id = data_columns[1];
            teamMakingInterception = data_columns[6]; // Defensive team, who will be on offense for the next drive.

            // Advance to the next line, the start of the intercepting team's offense.
            data_columns = AdvanceAndGetColumns(ref cursor, data);
            new_offense = data_columns[5];
            new_game_id = data_columns[1];

            while (new_offense == teamMakingInterception && new_game_id == game_id) // Stay in the loop if still the same offensive drive in the same game.
            {
                // Did they score? FIELD_GOAL_GOOD, TOUCHDOWN
                if (data[cursor].Contains("FIELD_GOAL_GOOD") || data[cursor].Contains("TOUCHDOWN"))
                {
                    // Don't record successful challenges.
                    if (!data[cursor].Contains("CHALLENGE_SUCCESSFUL"))
                    {
                        using (StreamWriter sw = File.AppendText(@"C:\results.csv"))
                        {
                            sw.WriteLine(data[cursor]);
                        }
                    }

                    // Break out of WHILE loop by maknig the offensive team names not match.
                    new_offense = "new play";
                }
                else
                {
                    // If not, advance to next play
                    data_columns = AdvanceAndGetColumns(ref cursor, data);
                    new_offense = data_columns[5];
                    new_game_id = data_columns[1];
                }
            }
            return cursor;
        }

        private String[] AdvanceAndGetColumns(ref long cursor, String[] data)
        {
            cursor++;
            String line = data[cursor];
            return line.Split('\t');
        }
    }
}
