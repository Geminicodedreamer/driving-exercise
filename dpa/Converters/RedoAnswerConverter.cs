using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using dpa.Library.Models;

namespace dpa.Converters;

public class RedoAnswerConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {

        //3_2   选择3 正确答案是2    1
        
        // Console.WriteLine(value);

        //判断选择的是否是正确答案
        if (value is null)
        {
            return Brushes.White;
        }
        else
        {
            string userAnswer = value.ToString().Split('_')[0].ToString();
            string realAnswer = value.ToString().Split('_')[1].ToString();
            string currentOption = parameter.ToString(); //
            Console.WriteLine("userAnswer"+userAnswer);
            Console.WriteLine("realAnswer"+realAnswer);
            Console.WriteLine("currentOption"+currentOption);
            if (realAnswer == currentOption)
            {
                return Brushes.Green;
            }
            if (userAnswer != realAnswer && userAnswer == currentOption)
            {

                return Brushes.Red;
            }
            return Brushes.White;
        }

    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Brushes.White;
    }
}