using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Calculator
{
    public class Calculator
    {
        // decimal? is a 'nullable' type
        // Value can be null when in an error state
        public decimal? value { get; private set; }
        private decimal operand1;
        private int maxDigits = 28;
        private StringBuilder inputString;
        private enum State
        {
            zeroed,
            fixedOperand,
            buildingInteger,
            buildingDecimal,
            overflow,
            underflow,
            zeroDivisor,
            error
        }

        private State currentState;

        private enum Operation
        {
            none,
            add,
            subtract,
            multiply,
            divide,
            equals
        }

        private Operation currentOp;

        public Calculator()
        {
            Zero();
        }
        public Calculator(int digitsAllowed): this()
        {
            maxDigits = digitsAllowed;
        }

        public void DigitIn(string digit)
        {
            DebugValues($"Start DigitIn({digit})");
            int maxLength = maxDigits;
            switch (currentState)
            {
                case State.overflow:
                    return; // wait for clear
                case State.zeroDivisor:
                    return; // wait for clear 
                case State.error:
                    return; // wait for clear 
                case State.zeroed:
                    if (digit == "0") return; //ignore
                    currentState = State.buildingInteger;
                    break;
                case State.fixedOperand:
                    inputString = new StringBuilder(" ", maxDigits + 2);
                    currentState = State.buildingInteger;
                    break;
                case State.buildingInteger:
                    break;
                case State.buildingDecimal:
                    maxLength++; //allow space for decimal point
                    break;
                default:
                    break;
            }
            if (inputString.Length <= maxLength)
            {
                inputString.Append(digit);
                value = decimal.Parse(inputString.ToString());
            }
            DebugValues($"End DigitIn({digit})");
        }

        public void Zero()
        {
            value = 0M;
            currentState = State.zeroed;
            currentOp = Operation.none;
            // Allow space for sign and decimal point
            inputString = new StringBuilder(" ", maxDigits + 2);
        }



        // All objects have a default ToString method so we must override it
        public override string ToString()
        {
            string outputString;
            if (value != null)
            {
                if (currentState == State.buildingDecimal)
                {
                    // while typing in a decimal value don't lose tra
                    outputString = inputString.ToString();
                }
                else
                {
                    // See how many digits are used by the integer part of a number
                    // value.Value ensures us a non-nullable decimal
                    int integerDigits = Decimal.Truncate(Math.Abs(value.Value)).ToString().Length;
                    int floatDigits = maxDigits - integerDigits;
                    // reduce any decimal part by rounding within unused digits
                    decimal roundedValue = Decimal.Round(value.Value, floatDigits);

                    // Convert decimal to a string 
                    // Remove trailing zeros (there will always be a decimal point)
                    string formatstring = "{0:F" + (floatDigits+1).ToString() + "}";
                    outputString = String.Format(formatstring, roundedValue).TrimEnd('0').TrimEnd('.');
                    
                    // Calculate maximum string length allowing for possible decimal point and minus sign
                    int maxLength = maxDigits;
                    if (outputString.Contains(".")) maxLength++;
                    if (outputString.Contains("-")) maxLength++;

                    // Trim to no more than maxdigits + sign and decimal point
                    if (outputString.Length > maxLength) outputString = outputString.Substring(0, maxLength);
                }

            }
            else
            {
                switch (currentState)
                {
                    case State.overflow:
                        outputString = "Overflow!";
                        break;
                    case State.underflow:
                        outputString = "Underflow!";
                        break;
                    case State.zeroDivisor:
                        outputString = "0 Divide!";
                        break;
                    case State.error:
                        outputString = "Error!";
                        break;
                    default:
                        outputString = "Undefined";
                        break;
                }

            }
            return outputString;
        }

        public void Equals()
        {
            DebugValues("Start Equals");
            Evaluate();
            currentOp = Operation.equals;
            DebugValues("End Equals");
        }

        private void Evaluate()
        {
            switch (currentOp)
            {
                case Operation.none:
                    break;
                case Operation.add:
                    value = operand1 + value;
                    currentState = State.fixedOperand;
                    currentState = DigitOverflowState(value);
                    break;
                case Operation.subtract:
                    value = operand1 - value;
                    currentState = State.fixedOperand;
                    currentState = DigitOverflowState(value);
                    break;
                case Operation.multiply:
                    value = operand1 * value;
                    currentState = State.fixedOperand;
                    currentState = DigitOverflowState(value);
                    break;
                case Operation.divide:
                    if (value!=0)
                    {
                        value = operand1 / value;
                        currentState = State.fixedOperand;
                        currentState = DigitOverflowState(value);
                    }
                    else
                    {
                        value = null;
                        currentState = State.zeroDivisor;
                    }
                    break;
                case Operation.equals:
                    break;
                default:
                    break;
            }
        }

        private void DebugValues(string whereAmI)
        {
            Debug.WriteLine(whereAmI);
            Debug.WriteLine($"\toperand1={operand1}");
            Debug.WriteLine($"\tcurrentOp={currentOp}");
            Debug.WriteLine($"\tcurrentState={currentState}");
            Debug.WriteLine($"\tinputString=\"{inputString}\"");
            Debug.WriteLine($"\tvalue={value}\n");
        }

        private State DigitOverflowState(decimal? testValue)
        {
            string maxString = new string('9', maxDigits);
            decimal maxValue = decimal.Parse(maxString);
            if (testValue > maxValue)
            {
                value = null;
                return State.overflow;
            }
            else if (testValue < -maxValue)
            {
                value = null;
                return State.underflow;
            }
            return currentState;
        }

        public void Add()
        {
            DebugValues("Start Add");
            Evaluate();
            if (value == null) return;
            else
            {
                operand1 = value.Value;
                currentOp = Operation.add;
                currentState = State.fixedOperand;
            }
            DebugValues("End Add");

        }
        public void Subtract()
        {
            DebugValues("Start Subtract");
            Evaluate();
            if (value == null) return;
            else
            {
                operand1 = value.Value;
                currentOp = Operation.subtract;
                currentState = State.fixedOperand;
            }
            DebugValues("End Subtract");
        }

        internal void Multiply()
        {
            DebugValues("Start Multiply");
            Evaluate();
            if (value == null) return;
            else
            {
                operand1 = value.Value;
                currentOp = Operation.multiply;
                currentState = State.fixedOperand;
            }
            DebugValues("End Multiply");
        }

        internal void Divide()
        {
            DebugValues("Start Divide");
            Evaluate();
            if (value == null) return;
            else
            {
                operand1 = value.Value;
                currentOp = Operation.divide;
                currentState = State.fixedOperand;
            }
            DebugValues("End Divide");
        }

        public void Point()
        {
            DebugValues("Start Point");
            if (currentState != State.buildingDecimal)
            {
                if (currentState == State.zeroed)
                {
                    inputString.Append("0");
                }
                else if (currentState == State.fixedOperand)
                {
                    inputString = new StringBuilder(" 0", maxDigits + 2);
                }
                inputString.Append(".");
                currentState = State.buildingDecimal;
            }
            DebugValues("End Point");
        }

        public void ChangeSign()
        {
            DebugValues("Start ChangeSign");
            if (value.HasValue) // not null
            {
                if (value > 0)
                {
                    inputString.Replace(' ', '-');
                }
                else if (value < 0)
                {
                    inputString.Replace('-', ' ');
                }
                //value = decimal.Parse(inputString.ToString());
                value = -value;
            }
            DebugValues("End ChangeSign");
        }

        public void Clear()
        {
            DebugValues("Start Clear");
            Zero();
            DebugValues("End Clear");

        }
    }
}
