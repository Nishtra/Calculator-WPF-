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

            // there was a number input
            if ((!isShowingResult || tbExpression.Text.Length == 0) && tbCurrentNumber.Text.Length > 0)
            {
                if (tbExpression.Text.Length == 0)
                {
                    operation = null;
                    rhsOperand = 0;
                    result = decimal.Parse(tbCurrentNumber.Text);
                }
                else
                    rhsOperand = decimal.Parse(tbCurrentNumber.Text);
                
                tbExpression.AppendText($"{tbCurrentNumber.Text} {nextOperation} ");
                tbCurrentNumber.Clear();

                CalcAndShowResult();
                operation = nextOperation;
            }
            // replace the last operation sign in expression
            else if (tbExpression.Text.Length > 0)
            {
                char lastOperationChar = tbExpression.Text.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Last()[0];
                
                if (nextOperation[0] != lastOperationChar)
                {
                    tbExpression.Text = $"{tbExpression.Text.Substring(0, tbExpression.Text.Length - 2)}{nextOperation} ";
                    operation = nextOperation;
                }
            }
        }

        private void CalcAndShowResult()
        {
            bool divBy0 = false;
            bool arithmeticOverflow = false;
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
                        if (rhsOperand != 0)
                            result /= rhsOperand;
                        else
                            divBy0 = true;
                        break;
                    default:
                        return;
                }
            }
            catch (ArithmeticException e)
            {
                arithmeticOverflow = true;
            }

            if (divBy0)
            {
                tbExpression.Clear();
                tbCurrentNumber.Text = "DIV BY 0";
                result = 0;
            }
            else if (arithmeticOverflow)
            {
                tbExpression.Clear();
                tbCurrentNumber.Text = "Arithmetic overflow";
                result = 0;
            }
            else
                tbCurrentNumber.Text = result.ToString("G29");
            isShowingResult = true;
        }

        private void OnEqualsBtn_Click(object sender, RoutedEventArgs e)
        {
            tbExpression.Clear();
            
            // new input was done
            if (!isShowingResult)
                // get new rhs operand from input, then calculate the result
                rhsOperand = decimal.Parse(tbCurrentNumber.Text);
            
            CalcAndShowResult();
        }

        private void OnFunctionalBtn_Click(object sender, RoutedEventArgs e)
        {
            string btnText = ((Button)sender).Content.ToString();
            switch (btnText)
            {
                case "C":
                    tbExpression.Clear();
                    tbCurrentNumber.Clear();
                    operation = null;
                    rhsOperand = 0;
                    result = 0;
                    tbCurrentNumber.Text = "0";
                    isShowingResult = true;
                    break;
                case "CE":
                    tbCurrentNumber.Clear();
                    tbCurrentNumber.Text = "0";
                    isShowingResult = true;
                    break;
                case "<":
                    if (tbCurrentNumber.Text.Length > 0)
                        tbCurrentNumber.Text = tbCurrentNumber.Text.Substring(0, tbCurrentNumber.Text.Length - 1);
                    break;
            }
        }
    }
}
