﻿using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WebScraperHLTV
{

    class Program
    {
        static int Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
               .WithParsed(o =>
               {
                   if (o.URL != null)
                   {
                       Console.WriteLine($"Going to parse the prediction thread {o.URL}");
                       PredictionThreadParser.ParsePredicitionThread(o.URL);
                   }
                   else if (o.Player != null && o.Predictions != null)
                   {
                       var predictions = GeneratePredicitions(o);
                       var scoreboard = GenerateScoreboard(o);

                       if (predictions == null || scoreboard == null)
                       {
                           Console.WriteLine("Cannot generate the scores as either the scoreboard or predictions are invalid.");
                       }
                       else
                       {
                           AddUpScores(o, predictions, scoreboard);
                       }
                   }
                   else
                   {
                       Console.WriteLine($"Invalid combination(or lack of) of arguments. If the URL is provided, it will be used. If not, Player, Predictions are all mandatory.");
                   }
               });

            return 0;
        }

        private static void AddUpScores(Options o, List<Prediction> predictions, Scoreboard scoreboard)
        {
            Console.WriteLine("Adding up scores");

            foreach (var prediction in predictions)
            {
                // TODO: Check the time.
                if (o.Player.Equals(prediction.Selection, StringComparison.InvariantCultureIgnoreCase))
                {
                    scoreboard.AddScore(prediction.Username);
                }
            }

            var buffer = new StringBuilder();
            var myList = scoreboard.Table.ToList();
            myList.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));

            foreach (var element in myList)
            {
                buffer.Append($"{element.Key},{element.Value}\n");
            }
            var resultsPath = Path.Combine(Directory.GetCurrentDirectory(), $"scoreboard-{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}.csv");

            File.WriteAllText(resultsPath, buffer.ToString());
            Console.WriteLine($"Done, result at {resultsPath}");
        }

        private static List<Prediction> GeneratePredicitions(Options o)
        {
            try
            {
                if (File.Exists(o.Predictions))
                {
                    List<Prediction> predictions = new List<Prediction>();

                    Console.WriteLine($"Reading {o.Predictions}");

                    var lines = File.ReadAllLines(o.Predictions);

                    foreach (var line in lines)
                    {
                        var prediction = Prediction.Generate(line);
                        if (prediction != null) predictions.Add(prediction);
                    }
                    return predictions;

                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Problem parsing {o.Predictions}");
            }
            return null;
        }

        private static Scoreboard GenerateScoreboard(Options o)
        {
            Scoreboard scoreboard = null;
            if (o.Score == null)
            {
                Console.WriteLine("There is no scoreboard file. Generating one.");
                scoreboard = new Scoreboard();
            }
            else
            {
                if (File.Exists(o.Score))
                {
                    try
                    {
                        Console.WriteLine($"Reading {o.Score}");
                        var scoreboardLines = File.ReadAllLines(o.Score);
                        scoreboard = Scoreboard.FromLines(scoreboardLines);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Problem parsing {o.Score}, Problem: {e.ToString()}");

                    }
                }
                else
                {
                    Console.WriteLine($"{o.Predictions} does not exist");
                    scoreboard = new Scoreboard();
                }
            }
            return scoreboard;
        }
    }
}
