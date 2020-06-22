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

        /// <summary>
        /// Indicates the current state of the calculator
        /// </summary>
        private enum State
        {
            zeroed,
            fixedOperand,
            buildingInteger,
            buildingDecimal,
            overflow,
            underflow,
            zeroDivisor,
            undefined,
            error
        }
        private State currentState;

        /// <summary>
        /// Indicates the last requested operation
        /// </summary>
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

        /// <summary>
        /// Default constructor, maxDigits uses default precision
        /// </summary>
        public Calculator()
        {
            // Initialise value and inputString as if clear was pressed
            Zero();
        }

        /// <summary>
        /// Constructor with specified .ToString() output precision
        /// <param name="digitsAllowed">Maximum digits allowed in .ToString() </param>
        public Calculator(int digitsAllowed): this()
        {
            maxDigits = digitsAllowed;

        }
        
        /// <summary>
        /// Set calculator to zero as if just switched on
        /// </summary>
        public void Zero()
        {
            value = 0M;
            currentState = State.zeroed;
            currentOp = Operation.none;
            // Allow space for sign and decimal point
            inputString = new StringBuilder(" ", maxDigits + 2);
        }

        /// <summary>
        /// Potentially add to the current inputString or start a new one
        /// </summary>
        /// <param name="digit">digit that could be added</param>
        public void DigitIn(string digit)
        {
            DebugValues($"Start DigitIn({digit})");
            // Note that maxLength is not cumulative between calls 
            int maxLength = maxDigits;
            // Note that if you use the switch code snippet and add an enum
            // between the parentheses all the case values get filled in 
            // automatically by intellisense!
            switch (currentState)
            {
                // error states do exactly the same thing so same body applies to all
                case State.overflow:
                case State.underflow:
                case State.zeroDivisor:
                case State.undefined:
                case State.error:
                    return; // ignore digits if in any of these error states
                case State.zeroed:
                    if (digit == "0") return; //ignore zero input if already zeroed
                    currentState = State.buildingInteger;
                    break;
                case State.fixedOperand:
                    // the displayed value is a result so don't add digits
                    // begin a new integer input instead
                    inputString = new StringBuilder(" ", maxDigits + 2);
                    currentState = State.buildingInteger;
                    break;
                case State.buildingInteger:
                    // digits are already being added
                    break;
                case State.buildingDecimal:
                    // allow for presence of the decimal point 
                    maxLength++;
                    // digits can be added continue to be added
                    break;
                default:
                    break;
            }
            // maxLength+1 to allow for initial space or negative sign
            if (inputString.Length < maxLength+1)
            {
                // StringBuilder uses append for concatenation
                inputString.Append(digit);
                // Hopefully the inputString is valid for conversion to decimal!
                value = decimal.Parse(inputString.ToString());
            }
            DebugValues($"End DigitIn({digit})");
        }

        /// <summary>
        /// Get a string representation of value or an error message if value is null
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string outputString;
            if (value != null) // value is valid
            {
                if (currentState == State.buildingDecimal)
                {
                    // while typing in a decimal value don't lose trailing zeros
                    outputString = inputString.ToString();
                }
                else if (inputString.ToString() == "-0")
                {
                    // we have started a negative number so allow -0
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

                    // Convert decimal to a string with at least one decimal digit
                    // Remove trailing zeros and trailing decimal point
                    string formatstring = "{0:F" + (floatDigits + 1).ToString() + "}";
                    outputString = String.Format(formatstring, roundedValue).TrimEnd('0').TrimEnd('.');

                    // Calculate maximum string length allowing for possible decimal point and minus sign
                    int maxLength = maxDigits;
                    if (outputString.Contains(".")) maxLength++;
                    if (outputString.Contains("-")) maxLength++;

                    // Trim to no more than maxdigits + sign and decimal point
                    if (outputString.Length > maxLength) outputString = outputString.Substring(0, maxLength);
                }
            }
            else // null value indicates an error state
            {
                // output string message depends on error state detected previously
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
                    case State.undefined:
                        outputString = "Undefined!";
                        break;
                    default:
                        outputString = "Error!";
                        break;
                }
            }
            return outputString;          
        }

        /// <summary>
        /// public interface to evaluate 
        /// </summary>
        public void Equals()
        {
            DebugValues("Start Equals");

            
            Evaluate();

            currentOp = Operation.equals;
            DebugValues("End Equals");
        }

        private void Evaluate()
        {
            // Try to complete the current calculation using
            // operand1, currentOp, value
            // placing the result into value
            // or setting an error state 
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
                        if (operand1 != 0)
                            currentState = State.zeroDivisor;
                        else
                            currentState = State.undefined;
                    }
                    break;
                case Operation.equals:
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Writes key field values to the Debug log
        /// </summary>
        /// <param name="whereAmI">Heading to be written above field values</param>
        private void DebugValues(string whereAmI)
        {
            Debug.WriteLine(whereAmI);
            // \t in a string is a tab character
            Debug.WriteLine($"\toperand1={operand1}");
            Debug.WriteLine($"\tcurrentOp={currentOp}");
            Debug.WriteLine($"\tcurrentState={currentState}");
            // \" in a string is a quote mark
            Debug.WriteLine($"\tinputString=\"{inputString}\"");
            // \n in a string is an extra newline
            Debug.WriteLine($"\tvalue={value}\n");
        }

        /// <summary>
        /// Check whether number is too long to fit within maxDigits
        /// </summary>
        /// <param name="testValue">number to be tested</param>
        /// <returns>unchange currentState value or an over/underflow state</returns>
        private State DigitOverflowState(decimal? testValue)
        {
            // biggest possible number within maxDigits
            // e.g. if maxdigits is 4, maxstring = "9999"
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
            // value returned should not change the state
            return currentState;
        }

        /// <summary>
        /// Complete any previous operation and prepare to add another value 
        /// </summary>
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

        /// <summary>
        /// Complete any previous operation and prepare to subtract another value 
        /// </summary>
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

        /// <summary>
        /// Complete any previous operation and prepare to multiply by another value 
        /// </summary>
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

        /// <summary>
        /// Complete any previous operation and prepare to divide by another value 
        /// </summary>
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


        /// <summary>
        /// Add a decimal point if appropriate
        /// </summary>
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

        /// <summary>
        /// Negate the current value 
        /// </summary>
        public void ChangeSign()
        {
            DebugValues("Start ChangeSign");
            if (value.HasValue) // not null
            {
                if (value > 0M)
                {
                    inputString.Replace(' ', '-');
                }
                else if (value < 0M)
                {
                    inputString.Replace('-', ' ');
                }
                value = -value;
            }
            DebugValues("End ChangeSign");
        }

        /// <summary>
        /// Reset the calculator
        /// </summary>
        public void Clear()
        {
            DebugValues("Start Clear");
            Zero();
            DebugValues("End Clear");

        }
    }
}
