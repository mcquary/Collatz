using System.Diagnostics;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;

/*
MIT License

Copyright (c) 2025 Kevin McQuary

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 */
namespace Collatz
{
    /// <summary>
    /// Author: Kevin McQuary
    /// Analyze any size binary string through the Collatz System.
    /// Input only limited by RAM available to the application
    /// 
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            RunConsole();
        }

        static bool IsValidBase2(string input)
        {
            Regex reg = new Regex("[0-1]*");
            return reg.IsMatch(input);
        }
        static bool IsValidPositiveBase10(string input)
        {
            Regex reg = new Regex("[0-9]*");
            return reg.IsMatch(input);
        }

        static void RunConsole()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Operations available:");
            sb.AppendLine("   0) To Process a base 2 number");
            sb.AppendLine("   1) To Process a base 10 number");
            sb.AppendLine("   2) To Process some 2^N-1 (Enter N base 10 value instead of supplying input");
            sb.AppendLine("Output will be generated to markdown file to prevent console truncation and performance impacts");
            sb.AppendLine("Type x to exit");
            while (true)
            {
                Console.WriteLine($"This is a binary simulation of the Collatz Conjecture written by Kevin McQuary");
                Console.WriteLine(sb.ToString());
                var typeSelection = Console.ReadLine().Trim();
                switch (typeSelection)
                {
                    case "0":
                    case "1":
                    case "2":
                        break;
                    case "x":
                        return;
                    default:
                        Console.WriteLine("Invalid selection.");
                        typeSelection = "";
                        break;
                }

                if (!string.IsNullOrEmpty(typeSelection))
                {
                    var inputValue = "";
                    if (typeSelection == "1")
                        Console.WriteLine("Enter Decimal Number:");
                    else
                        Console.WriteLine("Enter Binary Number:");
                    inputValue = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(inputValue))
                        continue;
                    switch (typeSelection)
                    {
                        case "0":
                            if (!IsValidBase2(inputValue))
                            {
                                inputValue = "";
                                Console.WriteLine("Invalid input value, not base 2. Format is 1010101");
                            }
                            break;
                        case "1":
                            if (!IsValidPositiveBase10(inputValue))
                            {
                                inputValue = "";
                                Console.WriteLine("Invalid input value, not base 10. Format is digits 0-9 with no commas, decimals or negative numbers.");
                            }
                            break;
                        case "x":
                            return;
                    }
                    var sequence = new McQuaryCollatzSystem();
                    Stopwatch stopwatch = new Stopwatch();

                    inputValue = inputValue.Trim();
                    var includeGraph = false;
                    if (!string.IsNullOrEmpty(inputValue))
                    {
                        var doIncludeGraph = false;
                        
                        switch (typeSelection)
                        {
                            case "0": //binary
                                break;
                            case "1":
                                BigInteger other = BigInteger.Parse(inputValue);
                                var sbInput = new StringBuilder();
                                while (other > 0)
                                {
                                    var output = other % 2;
                                    other >>= 1;
                                    if (output == 0)
                                        sbInput.Append("0");
                                    else
                                        sbInput.Append("1");
                                }
                                inputValue = string.Join("", sbInput.ToString().Reverse());
                                break;
                            case "2":
                                var val = BigInteger.Parse(inputValue);
                                var item = new StringBuilder();
                                for (var i = 0; i < val; i++)
                                {
                                    item.Append("1");
                                }
                                inputValue = item.ToString();
                                break;

                        }

                        Console.WriteLine($"Want to include the state graph? 1 for yes, 0 for no");
                        var choice = Console.ReadLine();
                        if (choice == "1")
                            doIncludeGraph = true;
                        var filename = $"output-{DateTime.Now.ToString("HHmmss")}.md";
                        File.WriteAllText(filename, "");
                        sequence.AnalyzeBinary(inputValue, filename, doIncludeGraph);
                        Console.WriteLine($"File output to {filename}");
                        Console.WriteLine("----------------------------------------------------------------------");
                        Console.WriteLine();

                    }
                }
            }
        }


        /// <summary>
        /// Author: Kevin McQuary
        /// Translate the functions defined by the Collatz system into binary operations for review
        /// if odd, x << 1 + x + 1... left shift X 1 bit, add to the original value X, add 1
        /// if even, x >> 1, right shift X 1 bit
        /// </summary>
        internal class McQuaryCollatzSystem
        {
            public List<List<string>> routes = new List<List<string>>();
            /// <summary>
            /// Convert to big integer
            /// </summary>
            /// <param name="list"></param>
            /// <returns></returns>
            private BigInteger GetLong(List<bool> list)
            {
                BigInteger number = Convert.ToInt32(list[0]);
                for (int i = 1; i < list.Count; i++)
                {
                    number <<= 1;
                    if (list[i])
                        number += 1;
                }
                return number;
            }

            /// <summary>
            /// Binary number string to be converted to a bit list
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public List<bool> GetBinaryListFromBinaryString(string value)
            {
                if (string.IsNullOrWhiteSpace(value)
                || value.Contains("-")
                || value.Contains("."))
                    throw new Exception($"Invalid number format, positive integers only");

                var result = new List<bool>();
                for (var i = 0; i < value.Length; i++)
                {
                    if (value[i] == '1')
                        result.Add(true);
                    else
                        result.Add(false);
                }
                return result;
            }
            /// <summary>
            /// Take a list of bools that represent a bit string
            /// </summary>
            /// <param name="list"></param>
            /// <returns></returns>
            public string PrintBinaryString(List<bool> list)
            {
                var sb = new StringBuilder();
                foreach (var item in list)
                {
                    if (item)
                        sb.Append("1");
                    else
                        sb.Append("0");
                }

                return sb.ToString();
            }

            /// <summary>
            /// Supply a binary representation of a number to analyze. If it's too large, the decimal number representation may be disrupted, but binary values will output
            /// until the decimal representation can fit into a 64-bit integer
            /// </summary>
            /// <param name="number"></param>
            public void AnalyzeBinary(string number, string outputFileName, bool printGraph = false, bool skipOutput = false)
            {
                var list = GetBinaryListFromBinaryString(number);
                AnalyzeWithListOfBools(list, outputFileName, printGraph, skipOutput);
            }

            /// <summary>
            /// Analyze bit list representation
            /// </summary>
            /// <param name="list"></param>
            private void AnalyzeWithListOfBools(List<bool> list, string outputFileName, bool printGraph, bool skipOutput = false)
            {
                long step = 0;
                long evenCount = 0;
                long oddCount = 0;
                var originalSize = list.Count;
                var lastBitCount = list.Count;
                var maxSize = 0;
                long verifyStep = 0;
                var parities = new List<bool>();
                var route = new List<string>();
                BigInteger origNumber = 0;
                long reductions = 0;
                long growthSteps = 0;
                long stepTrendChangeStep = 0;
                var zerosIntroduced = 0;
                var startingZeros = 0;
                int i = 0;
                var sb = new StringBuilder();
                var sbRoutes = new StringBuilder();
                var routesFilename = $"graph-routes-{DateTime.Now.ToString("HHmmss")}.md";
                var maxBits = 0;
                long maxGrowthSteps = 0;
                var initialGrowthComplete = false;

                var decimalText = "";
                origNumber = GetLong(list);

                if (printGraph)
                {
                    sbRoutes.AppendLine("--------------------------------------------");
                    sbRoutes.AppendLine($"Directed Graph Traversals For {origNumber}");
                    sbRoutes.AppendLine("--------------------------------------------");

                    File.WriteAllText(routesFilename, "");
                }
                for (i = list.Count - 1; i >= 0; i--)
                {
                    if (list[i])
                        break;
                    reductions++;
                }

                for (; i >= 0; i--)
                {
                    if (!list[i]) // stop at first false
                        break;
                    growthSteps++;
                }
                
                

                if (!skipOutput)
                {
                    sb.AppendLine($"Input Number Decimal: {origNumber}");
                    sb.AppendLine("");
                    sb.AppendLine($"Input Number Binary: {PrintBinaryString(list)}");

                    if (growthSteps > 1)
                    {

                        stepTrendChangeStep = reductions + 2 * growthSteps - 1;
                        sb.AppendLine($"- **Forecast: Growth stops at step: \\({step + reductions + 2 * growthSteps - 1}\\); Step Number for next guaranteed multi-bit reduction: \\({step + reductions + 2 * growthSteps} ({reductions + 2 * growthSteps})\\)...**");
                        sb.AppendLine("---");
                    }
                }
                var startPrinting = false;
                while (true)
                {
                    if (!skipOutput)
                    {
                        BigInteger bitIntVal;
                        bitIntVal = GetLong(list);
                        decimalText = $" \\({GetLong(list)}_{{10}}\\)";
                        
                        sb.AppendLine($"- **Step \\({step + 1}\\): Decimal: {decimalText} = Binary: \\({PrintBinaryString(list)}_2\\)**");

                        if (step == stepTrendChangeStep)
                            initialGrowthComplete = true;
                    }

                    reductions = 0;
                    growthSteps = 0;
                    i = 0;
                    for (i = list.Count - 1; i >= 0; i--)
                    {
                        if (list[i])
                            break;
                        reductions++;
                    }
                    for (; i >= 0; i--)
                    {
                        if (!list[i])
                            break;
                        growthSteps++;
                    }
                    if (step == verifyStep && verifyStep > 0)
                    {
                        if (list[list.Count - 2] || list[list.Count - 1])
                        {
                            throw new Exception("2N-1 rule violated");
                        }
                    }

                    // // Even step
                    if (!list[list.Count - 1])
                    {
                        if (!skipOutput)
                        {
                            if (growthSteps > 1)
                            {
                                if (initialGrowthComplete)
                                {
                                    if (maxGrowthSteps < growthSteps)
                                        maxGrowthSteps = growthSteps;
                                }
                                sb.AppendLine($"**Forecast: Step Number for next guaranteed multi-bit reduction: \\({step + reductions + 2 * growthSteps} ({reductions + 2 * growthSteps})\\)...**");
                            }
                        }
                        evenCount++;
                        // Collatz Even Step... x / 2 in decimal, x >> 1 in binary operations
                        list.RemoveAt(list.Count - 1);
                        decimalText = $" \\({GetLong(list)}_{{10}}\\)";

                        if (!skipOutput)
                            sb.AppendLine($"    - Step \\({step}\\) Right Shift Result: {decimalText} - Binary Result: \\({PrintBinaryString(list)}_2 \\)");
                    }
                    else // Odd Step
                    {

                        oddCount++;
                        if (growthSteps > 1)
                        {
                            verifyStep = step + reductions + 2 * growthSteps - 1;
                            if (!skipOutput)
                            {
                                sb.AppendLine($"- **Forecast: Growth stops at step: \\({step + 2 * growthSteps - 1}\\); Step Number for next guaranteed multi-bit reduction: \\({step + 2 * growthSteps} ({2 * growthSteps})\\)...**");
                            }
                        }
                        
                        list = OddStep(list, sb, skipOutput);
                    }
                    step++;
                    if (!skipOutput)
                    {
                        if (printGraph)
                        {
                            var numeric = GetLong(list);
                            decimalText = $" \\({numeric}_{{10}}\\)";
                            // is hundreths place odd or even
                            var hundredthsPlace = (numeric / 100) % 10;
                            var centiparity = hundredthsPlace % 2 == 0;
                            //var routeString = $"Even ";
                            var routeString = $"Step \\({step}\\) Result: Even (\\({hundredthsPlace}\\))";

                            var mod100 = numeric % 100;
                            if (!centiparity)
                            {
                                routeString = $"Step \\({step}\\) Result: Odd (\\({hundredthsPlace}\\))";
                            }
                            routeString += $"${mod100.ToString().PadLeft(2, '0')}$,";
                            sbRoutes.Append(routeString);
                            if (sbRoutes.Length > 10000)
                            {
                                File.AppendAllText(routesFilename, sbRoutes.ToString());
                                sbRoutes.Clear();
                            }
                        }

                        if (maxBits < list.Count)
                            maxBits = list.Count;

                        if (!skipOutput)
                        {
                            if (sb.Length > 10000)
                            {
                                File.AppendAllText(outputFileName, sb.ToString());
                                sb.Clear();
                            }
                        }
                    }
                    if (list.Count == 1 && list[0])
                        break;

                    var bound = originalSize * 3;
                    if (bound < list.Count)
                    {
                        throw new Exception("Maximum system bound exceeded.");
                    }
                }
                //Console.WriteLine($"|{origNumber}|{originalSize}|{maxBits}|{3 * originalSize - maxBits}|");
                if (!skipOutput)
                {
                    if (printGraph)
                    {
                        if (sbRoutes.Length > 0)
                            File.AppendAllText(routesFilename, sbRoutes.ToString());

                    }

                    if (sb.Length > 0)
                        File.AppendAllText(outputFileName, sb.ToString());
                }
                else {
                    Console.WriteLine($"Processing for {origNumber} completed. Odd Steps: {oddCount} Even Steps: {evenCount}");
                }

            }

            /// <summary>
            /// Perform Odd operation for Collatz. 3x + 1 in decimal, x << 1 + x + 1 in binary operations
            /// </summary>
            /// <param name="number"></param>
            /// <returns></returns>
            private List<bool> OddStep(List<bool> number, StringBuilder sb, bool printOutput)
            {
                var carry = false;
                //pad length to match upcoming left shift
                var original = new List<bool>() { false };
                original.AddRange(number);
                //left shift
                number.Add(false);
                var decimalText = "";
                if (!printOutput)
                {
                    decimalText = $" \\({GetLong(number)}_{{10}}\\)";
                    sb.AppendLine($"    - Left Shift \\((2x)\\): Decimal Result: {decimalText} - Binary Result: \\({PrintBinaryString(number)}_2 \\)");
                }

                // result will be calculated right to left, but we will store left to right for ease and reverse at the end
                var result = new List<bool>();
                for (var i = number.Count - 1; i >= 0; i--)
                {
                    if (original[i] && number[i])
                    {
                        if (carry) // true && true && carry  + 1 -> 1 + 1 = 0 +1 Carry + 1 Existing carry... value 1, continue carry
                            result.Add(true);
                        else // true && true && !carry  + 1 -> 1 + 1 = 0 +1 Carry ... value 0, continue carry
                            result.Add(false);
                        carry = true;
                    }
                    else if ((original[i] && !number[i]) || (!original[i] && number[i]))
                    {
                        if (carry)// true && false && carry  -> 1 + 0 = 1 +1 Carry = 0 ... , continue carry
                            result.Add(false);
                        else
                        {// true && false -> 1 + 0 = 1  value 1
                            result.Add(true);
                        }
                    }
                    else
                    { // false false => if carry, set true, reset carry
                        if (carry)
                        {
                            result.Add(true);
                            carry = false;
                        }
                        else // set false
                            result.Add(false);
                    }
                }
                //carry bit was still true after calculation, add to bits
                if (carry)
                    result.Add(true);
                carry = false;
                //reverse the list so the bits are in the correct order
                result.Reverse();
                if (!printOutput)
                {
                    decimalText = $" \\({GetLong(result)}_{{10}}\\)";
                    sb.AppendLine($"    - Add \\(X\\): Decimal Result: {decimalText} - Binary Result: \\({PrintBinaryString(result)}_2 \\)");
                }
                // Add 1
                for (var i = result.Count - 1; i >= 0; i--)
                {
                    if (!result[i])
                    {
                        // value is 0
                        result[i] = true;
                        carry = false;
                        break;
                    }
                    else
                    {
                        // value is 1, carrying
                        result[i] = false;
                        carry = true;
                    }
                }
                //Cover the 0000 case
                if (carry)
                {
                    result.Insert(0, true);
                }
                if (!printOutput)
                {
                    decimalText = $" \\({GetLong(result)}_{{10}}\\)";
                    sb.AppendLine($"    - Add \\(1\\): Decimal Result: {decimalText} - Binary Result: \\({PrintBinaryString(result)}_2 \\)");
                }
                return result;
            }
        }
    }
}