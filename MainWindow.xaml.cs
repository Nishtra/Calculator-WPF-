using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Calc
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        decimal result = 0;
        string operation = null;
        decimal rhsOperand = 0;
        bool isShowingResult = true;

        public MainWindow()
        {
            InitializeComponent();

            tbCurrentNumber.Text = "0";
        }

        private void OnNumBtn_Click(object sender, RoutedEventArgs e)
        {
            string numStr = ((Button)sender).Content.ToString();

            if (isShowingResult)
            {
                tbCurrentNumber.Clear();
                isShowingResult = !isShowingResult;
            }

            // allow only one leading zero
            string decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            bool numStartsWith0 = tbCurrentNumber.Text.Length > 0 && tbCurrentNumber.Text.StartsWith("0");
            bool hasDecimalDot = tbCurrentNumber.Text.Length > 0 && tbCurrentNumber.Text.Contains(decimalSeparator);
            if (numStr == "0" && numStartsWith0 && !hasDecimalDot)
                return;
            
            // allow only one decimal separator
            if (numStr == ".")
            {
                if (hasDecimalDot)
                    return;
                else
                    numStr = decimalSeparator;
            }

            tbCurrentNumber.AppendText(numStr);
        }

        private void OnMathOperationBtn_Click(object sender, RoutedEventArgs e)
        {
            string nextOperation = ((Button)sender).Content.ToString();

            // reset operation after resetting expression (after the user presses Equals button)
            if (tbExpression.Text.Length == 0)
                operation = null;

            // replace the last operation sign in expression if the user hasn't yet entered a new number
            if (isShowingResult && tbExpression.Text.Length > 0)
            {
                char lastOperationChar = tbExpression.Text.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Last()[0];

                if (nextOperation[0] != lastOperationChar)
                {
                    tbExpression.Text = $"{tbExpression.Text.Substring(0, tbExpression.Text.Length - 2)}{nextOperation} ";

                    bool addBrackets = (lastOperationChar == '+' || lastOperationChar == '-')
                        && (nextOperation[0] == '*' || nextOperation[0] == '/');
                    bool removeBrackets = (lastOperationChar == '*' || lastOperationChar == '/')
                        && (nextOperation[0] == '+' || nextOperation[0] == '-');

                    if (addBrackets)
                    {
                        tbExpression.Text = tbExpression.Text.Insert(0, "(").Insert(tbExpression.Text.Length - 2, ")");
                    }
                    else if (removeBrackets)
                    {
                        tbExpression.Text = tbExpression.Text.Replace("(", "").Replace(")", "");
                    }
                }
            }
            // calculate result
            else
            {
                // parse input number
                // no operation to perform -> no lhs operand (result) -> parse input to lhs operand
                if (operation == null)
                    decimal.TryParse(tbCurrentNumber.Text, out result);
                else
                    decimal.TryParse(tbCurrentNumber.Text, out rhsOperand);

                CalcAndShowResult();

                // build expression
                if (operation == null)
                    tbExpression.Text = $"{result} {nextOperation} ";
                else
                {
                    if (nextOperation == "+" || nextOperation == "-")
                        tbExpression.AppendText($"{rhsOperand.ToString()} {nextOperation} ");
                    else if (nextOperation == "*" || nextOperation == "/")
                    {
                        tbExpression.Text = tbExpression.Text.Insert(0, "(");
                        tbExpression.AppendText($"{rhsOperand.ToString()}) {nextOperation} ");
                    }
                }
            }
            
            operation = nextOperation;
        }

        private void CalcResult()
        {
            try
            {
                switch (operation)
                {
                    case "+":
                        result += rhsOperand;
                        break;
                    case "-":
                        result -= rhsOperand;
                        break;
                    case "*":
                        result *= rhsOperand;
                        break;
                    case "/":
                        if (rhsOperand == 0)
                            throw new DivideByZeroException("DIV BY 0");
                        result /= rhsOperand;
                        break;
                    default:
                        return;
                }
            }
            catch (OverflowException)
            {
                throw new OverflowException("Arithmetic overflow");
            }
        }

        private void ShowResult(string errorMsg = null)
        {
            if (errorMsg == null)
                tbCurrentNumber.Text = result.ToString("G29");
            else
            {
                tbCurrentNumber.Text = errorMsg;
            }
            isShowingResult = true;
        }

        private void CalcAndShowResult()
        {
            string errorMsg = null;
            try
            {
                CalcResult();
            }
            catch (Exception xcp)
            {
                errorMsg = xcp.Message;
                tbExpression.Clear();
                result = 0;
                rhsOperand = 0;
            }
            ShowResult(errorMsg);
        }

        private void OnEqualsBtn_Click(object sender, RoutedEventArgs e)
        {
            // new input was done
            if (!isShowingResult)
                // get new rhs operand from input, then calculate the result
                decimal.TryParse(tbCurrentNumber.Text, out rhsOperand);

            CalcAndShowResult();

            tbExpression.Clear();
        }

        private void OnFunctionalBtn_Click(object sender, RoutedEventArgs e)
        {
            string btnText = ((Button)sender).Content.ToString();
            switch (btnText)
            {
                case "C":
                    tbExpression.Clear();
                    operation = null;
                    rhsOperand = 0;
                    result = 0;
                    ResetNumberInputField();
                    break;
                case "CE":
                    ResetNumberInputField();
                    break;
                case "<":
                    if (tbCurrentNumber.Text.Length == 1)
                        ResetNumberInputField();
                    else if (tbCurrentNumber.Text.Length > 0)
                        tbCurrentNumber.Text = tbCurrentNumber.Text.Substring(0, tbCurrentNumber.Text.Length - 1);
                    break;
            }
        }

        private void ResetNumberInputField()
        {
            tbCurrentNumber.Text = "0";
            isShowingResult = true;
        }
    }
}
