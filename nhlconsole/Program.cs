
using Microsoft.VisualBasic.FileIO;
using System.Linq.Dynamic.Core;
using System.Xml.Linq;

namespace nhlconsole {
    internal class Program {
        // take in query, return sorted list
        static List<csvRow> handleQuery(string query) {
            var list = csvParser.getRows().AsQueryable(); //cast to queryable to use dynamic linq
            var split = query.Split(' '); //ie { "GP", ">=", "50" }
            if (split.Length > 2) {
                Console.WriteLine("handle multiple queries");
            }

            //name = cols[0],
            //team = cols[1],
            //pos = cols[2],
            //gp = int.Parse(cols[3]),
            //g = int.Parse(cols[4]),
            //a = int.Parse(cols[5]),
            //p = int.Parse(cols[6]),
            //plusMinus = int.Parse(cols[7]),
            //pim = int.Parse(cols[8]),
            //pOverGp = float.TryParse(cols[9], out _) ? float.Parse(cols[9]) : -999, // this one can be "--", replace this with -999 then interpret that as --
            //ppg = int.Parse(cols[10]),
            //ppp = int.Parse(cols[11]),
            //shg = int.Parse(cols[12]),
            //shp = int.Parse(cols[13]),
            //gwg = int.Parse(cols[14]),
            //otg = int.Parse(cols[15]),
            //s = int.Parse(cols[16]),
            //sPercent = float.Parse(cols[17]),
            //toiOverGp = cols[18],
            //shiftsOverGp = float.Parse(cols[19]),
            //fowPercent = float.Parse(cols[20])

            var field = split[0];
            var op = split[1];
            var val = split[2];
            switch (field.ToLower()) { //convert certain values and throw an error if none match
                case "player":
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
                    throw new Exception($"{val} is not a valid column name");
            }

            var where = $"{field} {op} {val}";
            var res = list.Where(where);
            return res.ToList();
        }

        static void printResult(List<csvRow> res) {
            foreach (var r in res)
            {
                Console.WriteLine(r.name);
            }

        }
        static void Main(string[] args)
        {
            string query = "";
            do {
                Console.WriteLine("Enter a query ie \"GP >= 50\". Type e to exit");
                query = Console.ReadLine();
                
                if (query == "e") { break; }
                var rows = csvParser.getRows();
                try {
                    var res = rows;
                    if (query != "") { res = handleQuery(query); }
                    printResult(res);
                    Console.WriteLine($"original count: {rows.Count}, filtered count: {res.Count}");
                } catch {
                    Console.WriteLine("Invalid query");
                }
            } while (true);
        }

        public static class csvParser {
            private static List<csvRow> rows = new List<csvRow>();
            public static List<csvRow> getRows() {
                return rows;
            }

            static csvParser() { //static constructor is called once this class is used for the first time
                using (TextFieldParser textFieldParser = new TextFieldParser(@"..\..\..\players.csv")) {
                    textFieldParser.TextFieldType = FieldType.Delimited;
                    textFieldParser.SetDelimiters(",");
                    var doneFirstLine = false;
                    while (!textFieldParser.EndOfData) {
                        string[] cols = textFieldParser.ReadFields();
                        if (doneFirstLine == false) { //skip first line
                            doneFirstLine = true;
                            continue;
                        }
                        //big ugly statement
                        var row = new csvRow {
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


        public class csvRow { //just used to store data
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
    }
}
