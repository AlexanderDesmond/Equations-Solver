using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Assignment1
{
    /*
     * Custom data type to hold data about the variables, operators, and numbers.
     */ 
    public struct XCharacter
    {
        public string type, value;
        public int precedence;

        public Character(string type, string value, int precedence)
        {
            // Types (values): Variable (e.g. X), Number (e.g. 1, 44, -2), Operator (e.g. '+', '-'. '*', '/', '(', ')')
            this.type = type;
            this.value = value;

            // Precedence for operators: 2 (* and /), 1 (+ and -), or else 0
            this.precedence = precedence;
        }
    }

    /*
     * Class for a calculator which solves linear equations.
     */ 
    class Calculator
    {
        private string userInput;
        private char[] characters, leftSide, rightSide;

        private Queue<Character> leftOutput, rightOutput, combinedExpression;
        private Stack<Character> leftOperators, rightOperators;
        private Stack<int> leftResult, rightResult;

        /**
         * Default constructor.
         */ 
        public Calculator() {}

        /*
         * Main method.
         */ 
        static void Main(string[] args)
        {
            Calculator calculator = new Calculator();

            if (args.Length > 0)
            {
                Console.Write("calc ");
                calculator.userInput = String.Join("", args);

                if (calculator.ValidateEquation(calculator.userInput))
                    calculator.EquationSolver(calculator.userInput.Trim());
            }
            while(true)
            {
                Console.Write("calc ");
                calculator.userInput = Console.ReadLine();

                if (calculator.ValidateEquation(calculator.userInput))
                    calculator.EquationSolver(calculator.userInput.Trim());
            }            
        }

        /*
         * Validates some test cases.
         */ 
        private bool ValidateEquation(string equation)
        {
            bool valid = true;

            String s = equation.Replace(" ", "");

            if (!equation.Contains("X") && !equation.Contains("x"))
            {
                Console.WriteLine("Error: No variable (X) in equation!");
                valid = false;
            }
            if (!equation.Contains("="))
            {
                Console.WriteLine("Error: No equals (=) sign in equation!");
                valid = false;
            }
            if (isOperator(equation[equation.Length-1].ToString()) && equation[equation.Length - 1] != '(' && equation[equation.Length - 1] != ')')
            {
                Console.WriteLine("Error: Equation terminates with operator!");
                return false;
            }
            if (s[s.Length-1] == '0' && isOperator(s[s.Length - 2].ToString()))
            {
                Console.WriteLine("Error: Ends with '+-*/' 0");
                return false;
            }
            for (int i = 0; i < s.Length; ++i)
            {
                if ((isOperator(s[i].ToString()) && s[i] != '(' && s[i] != ')') && !isNumber(s[i-1].ToString()) && !isNumber(s[i+1].ToString()))
                {
                    Console.WriteLine("Error: Operator ('+i*/') not between numbers!");
                    return false;
                }
                if ((isOperator(s[i].ToString()) && s[i] != '(' && s[i] != ')') && ((isOperator(s[i-1].ToString()) && s[i-1] != '(' && s[i-1] != ')') || (isOperator(s[i+1].ToString()) && s[i+1] != '(' && s[i+1] != ')')) )
                {
                    Console.WriteLine("Error: Multiple operators in a row!");
                    return false;
                }
            }

            return valid;
        }

        /*
         * Displays the solution to the equation.
         */
        private void EquationSolver(string equation)
        {
            DeconstructEquation(equation);

            Console.WriteLine("X = {0}", ReturnResult());
        }

        /*
         * Formats the equation in preparation for easier splitting and solving.
         */
        private void DeconstructEquation(string equation)
        {
            //equation = Regex.Replace(equation, " ", "");
            equation = equation.Replace(" ", "");

            characters = new char[equation.Length];
            characters = equation.ToCharArray();

            SplitEquation();

            SolveEquation(ref leftOutput, ref leftResult);
            SolveEquation(ref rightOutput, ref rightResult);

            combinedExpression = new Queue<Character>();
        }

        /*
         * Splits the equation into its left side and its right side.
         */
        private void SplitEquation()
        {
            string left = "";
            string right = "";

            string[] expressions = userInput.Split('=');

            // Remove if statement for submission build!!!
            if (expressions.Length > 1)
            {
                left = expressions[0].Trim();
                right = expressions[1].Trim();
            }

            leftOutput = new Queue<Character>();
            rightOutput = new Queue<Character>();
            leftOperators = new Stack<Character>();
            rightOperators = new Stack<Character>();

            leftResult = new Stack<int>();
            rightResult = new Stack<int>();

            CreateStacks(ref leftOutput, ref leftOperators, left);
            CreateStacks(ref rightOutput, ref rightOperators, right);
        }

        /*
         * Uses the shunting-yard algorithm to create two queues, one to hold each side of the equation, in postfix notation.
         */
        private void CreateStacks(ref Queue<Character> output, ref Stack<Character> operators, string expression)
        {
            string temp = "";
            for (int i = 0; i < expression.Length; ++i)
            {
                if (expression[i] == ' ')
                    continue;
                else
                {
                    // If character is a number
                    if (isDigit(expression[i]) || (expression[i] == '-' && isDigit(expression[i + 1])))
                    {
                        // Add number to string
                        temp += expression[i];
                    }
                    else if (isVariable(expression[i].ToString()))
                    {
                        if (temp != "")
                            output.Enqueue(InitialiseCharacter(temp));
                        temp = "";

                        if (i > 0 && isDigit(expression[i - 1]))
                            operators.Push(InitialiseCharacter("*"));

                        output.Enqueue(InitialiseCharacter(expression[i].ToString()));
                    }
                    else if (isOperator(expression[i].ToString()))
                    {
                        // Push the string onto the output stack, and push the operator onto the operator stack
                        if (temp != "")
                            output.Enqueue(InitialiseCharacter(temp));
                        temp = "";

                        
                        while (operators.Count != 0 && operators.Peek().precedence >= getPrecedence(expression[i].ToString()) && operators.Peek().value != "(" && operators.Peek().value != ")")
                        {
                            string op;
                            op = operators.Pop().value;
                            output.Enqueue(InitialiseCharacter(op));
                        }

                        //if (operators.Count != 0 && operators.Peek().value != "(" && operators.Peek().value != ")")
                        operators.Push(InitialiseCharacter(expression[i].ToString()));

                    }
                    else if (expression[i] == '(')
                    {
                        if (i > 0 && isDigit(expression[i - 1]))
                            operators.Push(InitialiseCharacter("*"));

                        operators.Push(InitialiseCharacter(expression[i].ToString()));

                        //
                    }
                    else if (expression[i] == ')')
                    {
                        while (operators.Peek().value != "(")
                        {
                            string op;
                            op = operators.Pop().value;
                            output.Enqueue(InitialiseCharacter(op));
                        }
                        operators.Pop();
                    }
                }
            }
            if (temp != "")
                output.Enqueue(InitialiseCharacter(temp));
            temp = "";

            while (operators.Count != 0 && operators.Peek().value != "(" && operators.Peek().value != ")")
            {
                string op;
                op = operators.Pop().value;
                output.Enqueue(InitialiseCharacter(op));
            }
        }

        /*
         * Initialised an instance of the Character custom type.
         */
        private Character InitialiseCharacter(string s)
        {
            Character character;

            if (isNumber(s))
            {
                character = new Character("Number", s, 0);
            }
            else if (isOperator(s))
            {
                character = new Character("Operator", s, getPrecedence(s));
            }
            else
            {
                character = new Character("Variable", s, 0);
            }

            return character;
        }

        /*
         * Solves the equation.
         */
        private void SolveEquation(ref Queue<Character> output, ref Stack<int> result)
        {
            // BODMAS = Brackets -> orders -> division -> multiplication -> addition -> subtraction

            foreach (Character character in output)
            {
                if (isNumber(character.value))
                {
                    result.Push(getNumber(character.value));
                }
                else if (isOperator(character.value) && result.Count >= 2)
                {
                    if (character.value.Equals("*"))
                    {
                        result.Push(Multiplication(result.Pop(), result.Pop()));
                    }
                    if (character.value.Equals("/"))
                    {
                        int temp2 = result.Pop();
                        int temp1 = result.Pop();
                        result.Push(Division(temp1, temp2));
                    }
                    if (character.value.Equals("+"))
                    {
                        result.Push(Addition(result.Pop(), result.Pop()));
                    }
                    if (character.value.Equals("-"))
                    {
                        int temp2 = result.Pop();
                        int temp1 = result.Pop();
                        result.Push(Subtraction(temp1, temp2));
                    }
                }
            }
        }

        /*
         * Returns the result of the equation.
         */
        private string ReturnResult()
        {
            if (leftResult.Count() == 0 && (rightResult.Count == 1 && isNumber(rightResult.Peek().ToString())))
            {
                return rightResult.Peek().ToString();
            }
            if (rightResult.Count == 0 && (leftResult.Count == 1 && isNumber(leftResult.Peek().ToString())))
            {
                return leftResult.Peek().ToString();
            }

            return "";
        }

        /*
         * Divides two numbers.
         */
        private int Division(int num1, int num2)
        {
            return num1 / num2;
        }

        /*
         * Multiplies two numbers.
         */
        private int Multiplication(int num1, int num2)
        {
            return num1 * num2;
        }

        /*
         * Adds two numbers.
         */
        private int Addition(int num1, int num2)
        {

            return num1 + num2;
        }

        /*
         * Subtracts two numbers.
         */
        private int Subtraction(int num1, int num2)
        {

            return num1 - num2;
        }

        /*
         *  Returns true if the input is a variable.
         */
        private bool isVariable(string var)
        {
            if (var == "X" || var == "x")
                return true;
            return false;
        }

        /*
         *  Returns true if the input is a number.
         */
        private bool isDigit(char num)
        {
            return Char.IsDigit(num);
        }

        /*
         *  Returns true if the input is a number.
         */
        private bool isNumber(string num)
        {
            int test;
            return int.TryParse(num, out test);
        }

        /*
         *  Returns true if the input is an operator.
         */
        private bool isOperator(string op)
        {
            switch (op)
            {
                case "(":
                    return true;
                case ")":
                    return true;
                case "*":
                    return true;
                case "/":
                    return true;
                case "+":
                    return true;
                case "-":
                    return true;
                default:
                    return false;
            }
        }

        /*
         *  Returns the precedence of an operator.
         */
        private int getPrecedence(string op)
        {
            switch (op)
            {
                case "*":
                    return 2;
                case "/":
                    return 2;
                case "+":
                    return 1;
                case "-":
                    return 1;
                default:
                    return 0;
            }
        }

        /*
         *  Converts a string into a number and returns it.
         */
        private int getNumber(string num)
        {   
                return Convert.ToInt32(num);
        }
    }
}
