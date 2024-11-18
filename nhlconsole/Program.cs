using Microsoft.VisualBasic.FileIO;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;

namespace nhlconsole
{
    internal class Program
    {
        static List<csvRow> handleQuery(IQueryable<csvRow> list, string query)
        {
            var split = query.Split(' '); //ie { "GP", ">=", "50" }

            var where = $"{split[0]} {split[1]} {split[2]}";
            var res = list.Where(where);
            return res.ToList();
        }

        static void printResult(List<csvRow> list)
        {
            Console.WriteLine($"Filtered results [{list.Count} rows]:");
            foreach (var r in list)
            {
            }
        }

        static void Main(string[] args)
        {
            string query = "";
            Console.WriteLine("Maximize the window and press ctrl + - to see all columns");
            var filterHandler = new CsvFilterHandler(csvParser.getRows()); 
            do
            {
                Console.WriteLine("=========================================================================");
                Console.WriteLine("Enter one or multiple queries separated by a comma ie 'gp > 10, gp < 50'. " +
                    "\nType h for help" +
                    "\nPress s to sort" +
                    "\nJust press enter to print all data" +
                    "\nType e to exit");
                query = Console.ReadLine().ToLower();
                if (query == "e") { break; }
                if (query == "h") {
                    Console.WriteLine("\nPossible column names (not case sensitive) are:" +
                        "\nName,Team,Pos,GP,G,A,P,+/-,PIM,P/GP,PPG,PPP,SHG,SHP,GWG,OTG,S,S%,TOI/GP,Shifts/GP,FOW%" +
                        "\nPossible operators are <, <=, =, >, >=");
                    continue; 
                }
                if (query == "") {
                    filterHandler.printAllRows();
                }
                if (query == "s") {
                    var valid = false;
                    var col = -1;
                    var dir = -1;
                    do {
                        try {
                            Console.WriteLine("Which column do you want to sort by");
                            Console.WriteLine("0   | 1   | 2  | 3 | 4| 5| 6| 7  | 8  | 9   | 10 | 11 | 12 | 13 | 14 | 15 | 16| 17| 18    | 19       | 20");
                            Console.WriteLine("Name| Team| Pos| GP| G| A| P| +/-| PIM| P/GP| PPG| PPP| SHG| SHP| GWG| OTG| S | S%| TOI/GP| Shifts/GP| FOW%");
                            col = int.Parse(Console.ReadLine());
                            if (col < 0 || col > 20) {
                                throw new Exception("Select a valid column (0 - 20)");
                            }
                            Console.WriteLine("In which direction");
                            Console.WriteLine("0         | 1");
                            Console.WriteLine("Ascending | Descending");
                            dir = int.Parse(Console.ReadLine());
                            if (dir != 0 && dir != 1) {
                                throw new Exception("Select a valid direction (0 or 1)");
                            }
                            valid = true;
                        } catch (Exception e) {
                            Console.WriteLine(e.Message);
                            valid = false;
                        }
                    } while (!valid);
                    filterHandler.sortResultsAndReprint(col, dir);
                } else {
                    filterHandler = new CsvFilterHandler(csvParser.getRows()); //a new query is being done so the previous results can be discarded
                    filterHandler.ApplyFilters(query);
                }
                // Use CsvFilterHandler to handle the input query
            } while (true);
        }

        public static class csvParser
        {
            private static List<csvRow> rows = new List<csvRow>();
            public static List<csvRow> getRows()
            {
                return rows;
            }

            // Static constructor is called once this class is used for the first time
            static csvParser()
            {
                using (TextFieldParser textFieldParser = new TextFieldParser(@"..\..\..\players.csv"))
                {
                    textFieldParser.TextFieldType = FieldType.Delimited;
                    textFieldParser.SetDelimiters(",");
                    var doneFirstLine = false;
                    while (!textFieldParser.EndOfData)
                    {
                        string[] cols = textFieldParser.ReadFields();
                        if (doneFirstLine == false)
                        { //skip first line
                            doneFirstLine = true;
                            continue;
                        }
                        //big ugly statement
                        var row = new csvRow
                        {
                            //Name,Team,Pos,GP,G,A,P,+/-,PIM,P/GP,PPG,PPP,SHG,SHP,GWG,OTG,S,S%,TOI/GP,Shifts/GP,FOW%
                            name = cols[0],
                            team = cols[1],
                            pos = cols[2],
                            gp = int.Parse(cols[3]),
                            g = int.Parse(cols[4]),
                            a = int.Parse(cols[5]),
                            p = int.Parse(cols[6]),
                            plusMinus = int.Parse(cols[7]),
                            pim = int.Parse(cols[8]),
                            pOverGp = float.TryParse(cols[9], out _) ? float.Parse(cols[9]) : -999, // this one can be "--", replace this with -999 then interpret that as --
                            ppg = int.Parse(cols[10]),
                            ppp = int.Parse(cols[11]),
                            shg = int.Parse(cols[12]),
                            shp = int.Parse(cols[13]),
                            gwg = int.Parse(cols[14]),
                            otg = int.Parse(cols[15]),
                            s = int.Parse(cols[16]),
                            sPercent = float.Parse(cols[17]),
                            toiOverGp = cols[18],
                            shiftsOverGp = float.Parse(cols[19]),
                            fowPercent = float.Parse(cols[20])
                        };
                        rows.Add(row);
                    }
                }
            }
        }


        public class csvRow
        {
            public string name;
            public string team;
            public string pos; //could change to enum 
            public int gp;
            public int g;
            public int a;
            public int p;
            public int plusMinus;
            public int pim;
            public float pOverGp;
            public int ppg;
            public int ppp;
            public int shg;
            public int shp;
            public int gwg;
            public int otg;
            public int s;
            public float sPercent;
            public string toiOverGp;
            public float shiftsOverGp;
            public float fowPercent;
        }

        //Print data corresponding to filters applied
        public class CsvFilterHandler
        {
            private List<csvRow> rows;
            public CsvFilterHandler(List<csvRow> rows)
            {
                this.rows = rows;
            }

            public void sortResultsAndReprint(int col, int order) { //col 0 - 20, order 0:asc 1:desc
                string[] columnNames = { "name", "team", "pos", "gp", "g", "a", "p", "plusMinus", "pim", "pOverGp", "ppg", "ppp", "shg", "shp", "gwg", "otg", "s", "sPercent", "toiOverGp", "shiftsOverGp", "fowPercent" };
                rows = rows.AsQueryable().OrderBy(columnNames[col]).ToList();
                if (order == 1) {
                    rows.Reverse();
                }
                printAllRows();
            }

            // Apply the filters based on the user's input
            public void ApplyFilters(string query)
            {
                var expressions = query.Split(","); // Get list of expressions if there are multiple
                for (int i = 0; i < expressions.Length; i++) // Keep modifying rows until done with all the expressions
                {
                    var expr = expressions[i].Trim();
                    var prevLength = rows.Count;
                    try
                    {
                        rows = FilterRows(rows.AsQueryable(), expr); // Apply the filter
                        Console.WriteLine($"Expression '{expr}' filtered out {prevLength - rows.Count} from remaining total {prevLength}");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Invalid query: '{e.Message}'"); // Catch and report invalid queries
                        return;
                    }
                }
                PrintResults(rows); // Output the filtered results
            }

            // Filters the rows based on the given expression
            private List<csvRow> FilterRows(IQueryable<csvRow> queryableRows, string expression)
            {
                var split = expression.Split(" ");
                if (split.Length != 3 ) {
                    throw new Exception("Expression needs to follow the format [column] [operator] [value]");
                }
                var field = split[0].Trim();
                var op = split[1].Trim();
                var val = split[2].Trim();
                switch (field.ToLower()) { //convert certain values and throw an error if none match
                    case "name":
                    case "team":
                    case "pos":
                    case "gp":
                    case "g":
                    case "a":
                    case "p":
                    case "pim":
                    case "ppg":
                    case "ppp":
                    case "shg":
                    case "shp":
                    case "gwg":
                    case "otg":
                    case "s":
                        break;
                    case "+/-":
                        field = "plusMinus";
                        break;
                    case "p/gp":
                        field = "pOverGp";
                        if (val == "----") { val = "-999.0"; }
                        break;
                    case "s%":
                        field = "sPercent";
                        break;
                    case "toi/gp":
                        field = "toiOverGp";
                        break;
                    case "shifts/gp":
                        field = "shiftsOverGp";
                        break;
                    case "fow%":
                        field = "fowPercent";
                        break;
                    default:
                        throw new Exception($"{field} is not a valid column name");
                }

                var where = $"{field} {op} {val}";
                return queryableRows.Where(where).ToList();
            }

            public void printAllRows() {
                PrintResults(rows);
            }
            private void PrintResults(List<csvRow> filteredRows)
            {
                // Print the header row with appropriate spacing
                Console.WriteLine("| {0,-24} | {1,-8} | {2,-3} | {3,3} | {4,2} | {5,2} | {6,3} | {7,4} | {8,4} | {9,4} | {10,3} | {11,3} | {12,3} | {13,3} | {14,3} | {15,3} | {16,3} | {17,5} | {18,-7} | {19,-11} | {20,-5} |",
                                    "Player", "Team", "Pos", "GP", "G", "A", "P", "+/-", "PIM", "P/GP", "PPG", "PPP", "SHG", "SHP", "GWG", "OTG", "S", "S%", "TOI/GP", "Shifts/GP", "FOW%");

                // Print a separator line for the table
                Console.WriteLine(new string('-', 170));

                // Loop through each row and print the data, formatted in a table
                foreach (var row in filteredRows)
                {
                    Console.WriteLine("| {0,-24} | {1,-8} | {2,-3} | {3,3} | {4,2} | {5,2} | {6,3} | {7,4} | {8,4} | {9,4:F2} | {10,3} | {11,3} | {12,3} | {13,3} | {14,3} | {15,3} | {16,3} | {17,5:F2} | {18,-11} | {19,-7:F2} | {20,-5:F2} |",
                                    row.name, row.team, row.pos, row.gp, row.g, row.a, row.p, row.plusMinus, row.pim, row.pOverGp == -999.0 ? "----" : row.pOverGp , row.ppg, row.ppp, row.shg, row.shp, row.gwg, row.otg, row.s, row.sPercent, row.toiOverGp, row.shiftsOverGp, row.fowPercent);
                }
            }

        }
    }
}
