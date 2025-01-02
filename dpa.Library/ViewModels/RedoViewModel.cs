using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using dpa.Library.Helpers;
using dpa.Library.Models;
using dpa.Library.Services;
using System.Diagnostics;

namespace dpa.Library.ViewModels;

public class RedoViewModel : ViewModelBase
{
    private readonly IPoetryStorage _poetryStorage;

    // 当前错题
    public Exercise CurrentQuestion { get; set; }

    // 题目索引
    private int _currentIndex = 0;
    public int CurrentIndex
    {
        get => _currentIndex;
        set => SetProperty(ref _currentIndex, value);
    }

    // 当前题目的索引（展示给用户）
    public int CurrentQuestionIndex { get; set; }

    // 题目列表（错题列表）
    private ObservableCollection<Exercise> _exerciseQuestions;
    public ObservableCollection<Exercise> ExerciseQuestions
    {
        get => _exerciseQuestions;
        set => SetProperty(ref _exerciseQuestions, value);
    }

    // 用户答案
    private string _userAnswer;
    public string UserAnswer
    {
        get => _userAnswer;
        set => SetProperty(ref _userAnswer, value);
    }
    
    private string _userAnswer_Answer;
    public string UserAnswer_Answer
    {
        get => _userAnswer_Answer;
        set => SetProperty(ref _userAnswer_Answer, value);
    }

    private string _correctAnswer;
    public string CorrectAnswer
    {
        get => _correctAnswer;
        set => SetProperty(ref _correctAnswer, value);
    }

    private bool _isAnswerVisible;
    public bool IsAnswerVisible
    {
        get => _isAnswerVisible;
        set => SetProperty(ref _isAnswerVisible, value);
    }

    private string _explanation;
    public string Explanation
    {
        get => _explanation;
        set => SetProperty(ref _explanation, value);
    }

    public ICommand RadioSelectionCommand { get; }

    private void OnRadioSelection(string selectedValue)
    {
        UserAnswer = selectedValue;
        Console.WriteLine(CorrectAnswer);
        UserAnswer_Answer = $"{UserAnswer}_{CorrectAnswer}"; // 设置用户答案和正确答案的组合
        
        
        Explanation = CurrentQuestion.explains; // 获取解析

        IsAnswerVisible = true; // 显示答案和解析
    }
    // 额外的方法来重置状态
    public void Reset()
    {
        UserAnswer = string.Empty; // 清空用户答案
        IsAnswerVisible = false; // 隐藏答案和解析
        Explanation = string.Empty; // 清空解析
    }

    public ICommand ExitRedoCommand { get; }

    // 构造函数，初始化命令和加载错题
    public RedoViewModel(IPoetryStorage poetryStorage, Exercise exercise = null)
    {
        CorrectAnswer = exercise?.answer;
        _poetryStorage = poetryStorage;
        ExitRedoCommand = new RelayCommand(ExitRedo);

        // 如果传入了错题，则直接设置
        if (exercise != null)
        {
            CurrentQuestion = exercise;
            LoadExerciseQuestions(); // 如果需要加载其他错题
        }
        else
        {
            LoadExerciseQuestions(); // 加载错题
        }
        
        RadioSelectionCommand = new RelayCommand<string>(OnRadioSelection);
        Reset(); // 初始化时重置状态
    }

    // 加载错题列表
    private async void LoadExerciseQuestions()
    {
         try 
        {
            // 添加日志
            Debug.WriteLine("开始加载错题");
            
            var exercises = await _poetryStorage.GetExerciseQuestionsAsync(null, 0, 1);
            if (exercises.Count > 0)
            {
                ExerciseQuestions = new ObservableCollection<Exercise>(exercises);
                CurrentQuestion = exercises[0];
                CurrentQuestionIndex = CurrentIndex + 1;
                
                // 添加日志
                Debug.WriteLine($"加载到 {exercises.Count} 道错题");
            }
            else
            {
                Debug.WriteLine("没有找到错题");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"加载错题时出错: {ex.Message}");
        }
    }
    
    public string GetOptionColor(string option)
    {
        if (IsAnswerVisible)
        {
            return option == CorrectAnswer ? "Green" : "Red"; // 正确答案为绿色，错误答案为红色
        }
        return "Transparent"; // 默认透明
    }


    // 退出重做，返回错题回顾页面
            private void ExitRedo()
    {
        // 导航到错题回顾页面
        // 需要根据您的导航机制来实现
    }
}

