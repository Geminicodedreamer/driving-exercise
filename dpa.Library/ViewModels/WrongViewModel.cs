using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using dpa.Library.Models;
using dpa.Library.Services;

namespace dpa.Library.ViewModels;

public class WrongViewModel : ViewModelBase
{
    private readonly IPoetryStorage _poetryStorage;
    private readonly IWrongNavigationService _wrongNavigationService;
    
    public WrongViewModel(IPoetryStorage poetryStorage , IWrongNavigationService wrongNavigationService)
    {
        _poetryStorage = poetryStorage;
        _wrongNavigationService = wrongNavigationService;
    
        OnInitializedCommand = new AsyncRelayCommand(LoadExerciseQuestions);
        RedoQuestionCommand = new RelayCommand(RedoQuestion);
    
        Task.Run(async () => await LoadExerciseQuestions());
    }
    
    public ObservableCollection<ViewModelBase> ContentStack { get; } = [];
    private ViewModelBase _content;
    public ViewModelBase Content {
        get => _content;
        private set => SetProperty(ref _content, value);
    }
    public void PushContent(ViewModelBase content) =>
        ContentStack.Insert(0, Content = content);
    
    public ICommand OnQuestionClickedCommand => new RelayCommand<int>((id) => OnQuestionClicked(id));
    private int _currentIndex = 0;
    public int CurrentIndex
    {
        get => _currentIndex;
        set
        {
            if (SetProperty(ref _currentIndex, value))  // 只有值真正变化时才触发
            {
                LoadCurrentQuestion();  // 分离加载逻辑
            }
        }
    }
    private async void LoadCurrentQuestion()
    {
        if (_exerciseQuestions != null && _currentIndex >= 0 && _currentIndex < _exerciseQuestions.Count)
        {
            int id = _exerciseQuestions[_currentIndex].Id;
            await LoadQuestionDetails(id);
        }
    }

    private async Task LoadQuestionDetails(int id)
    {
        CurrentQuestion = await _poetryStorage.GetWrongQuestionByIdAsync(id);
        CurrentQuestionIndex = _currentIndex + 1;
    }

    private ICommand _previousQuestionCommand;
    public ICommand PreviousQuestionCommand
    {
        get
        {
            if (_previousQuestionCommand == null)
            {
                _previousQuestionCommand = new AsyncRelayCommand(PreviousQuestion);
            }
            return _previousQuestionCommand;
        }
    }

    private ICommand _nextQuestionCommand;
    public ICommand NextQuestionCommand
    {
        get
        {
            if (_nextQuestionCommand == null)
            {
                _nextQuestionCommand = new AsyncRelayCommand(NextQuestion);
            }
            return _nextQuestionCommand;
        }
    }
    
    private int _currentQuestionIndex = 1;
    public int CurrentQuestionIndex
    {
        get => _currentQuestionIndex;
        set => SetProperty(ref _currentQuestionIndex, value);
    }

    private int _totalQuestions;
    public int TotalQuestions
    {
        get => _totalQuestions;
        set => SetProperty(ref _totalQuestions, value);
    }

    public async Task PreviousQuestion()
    {
        if (CurrentIndex > 0)
        {
            CurrentIndex--;
            CurrentQuestionIndex = CurrentIndex + 1;
            CurrentQuestion = ExerciseQuestions[CurrentIndex];
        }
    }

    public async Task NextQuestion()
    {
        if (CurrentIndex < _exerciseQuestions.Count - 1)
        {
            CurrentIndex++;
            CurrentQuestionIndex = CurrentIndex + 1;
            CurrentQuestion = ExerciseQuestions[CurrentIndex];
        }
    }
    // 添加这个属性
    public ICommand OnInitializedCommand { get; }

    // ObservableCollection 用于绑定到 UI
    private ObservableCollection<Exercise> _exerciseQuestions;
    public ObservableCollection<Exercise> ExerciseQuestions
    {
        get => _exerciseQuestions;
        set => SetProperty(ref _exerciseQuestions, value);
    }

    private string _status;
    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    // 状态常量
    public const string Loading = "正在载入";
    public const string NoResult = "没有满足条件的结果";
    public const string NoMoreResult = "没有更多结果";

    // 分页大小
    public const int PageSize = 1000;

    // 加载问题列表的方法
    public async Task LoadExerciseQuestions()
    {
        Status = Loading;

        var exercises = await _poetryStorage.GetExerciseQuestionsAsync(null, 0, PageSize);

        if (exercises.Count == 0)
        {
            Status = NoResult;
        }
        else
        {
            for (int i = 0; i < exercises.Count; i++)
            {
                var exercise = exercises[i];
                if (exercise.question.Length > 8)
                {
                    exercise.question = exercise.question.Substring(0, 8) + "...";
                }
                exercise.question = $"（{(i + 1)}）{exercise.question}";
            }

            ExerciseQuestions = new ObservableCollection<Exercise>(exercises);
            Status = string.Empty;

            CurrentIndex = 0;
            TotalQuestions = exercises.Count;
        }
        if (ExerciseQuestions != null)
        {
            OnQuestionClicked(ExerciseQuestions[0].Id);
        }
    }
    
    private Exercise _currentQuestion;
    public Exercise CurrentQuestion
    {
        get => _currentQuestion;
        set => SetProperty(ref _currentQuestion, value);
    }

    public async void OnQuestionClicked(int id)
    {
        // 找到当前题目在数组中的索引
        int index = -1;
        for (int i = 0; i < _exerciseQuestions.Count; i++)
        {
            if (_exerciseQuestions[i].Id == id)
            {
                index = i;
                break;
            }
        }
        // 获取完整的题目信息
        CurrentIndex = index;
        CurrentQuestionIndex = index + 1;
        CurrentQuestion = await _poetryStorage.GetWrongQuestionByIdAsync(id);
        // Console.WriteLine(CurrentQuestion.url);
    }
    
    // 删除错题
    public ICommand DeleteQuestionCommand => new RelayCommand(DeleteCurrentQuestion);

    private async void DeleteCurrentQuestion()
    {
        if (CurrentQuestion == null) return;

        // 删除当前错题
        await _poetryStorage.DeleteWrongQuestionAsync(CurrentQuestion.Id);

        // 从错题列表中移除当前错题
        var deletedQuestion = ExerciseQuestions.FirstOrDefault(q => q.Id == CurrentQuestion.Id);
        if (deletedQuestion != null)
        {
            ExerciseQuestions.Remove(deletedQuestion);
        }

        // 如果删除的是最后一题，跳转到第一题，否则跳转到下一题
        if (ExerciseQuestions.Count > 0)
        {
            // 更新题目数量
            TotalQuestions = ExerciseQuestions.Count;

            // 确保在删除后如果是最后一题，跳转到第一题
            if (CurrentIndex >= ExerciseQuestions.Count)
            {
                CurrentIndex = 0; // 如果是最后一题，则跳转到第一题
            }

            OnQuestionClicked(ExerciseQuestions[CurrentIndex].Id);
        }
        else
        {
            // 如果没有错题了，清空当前题目
            CurrentQuestion = null;
            CurrentQuestionIndex = 0;
        }

        // 更新左侧题目序号
        UpdateQuestionNumbers();
    }

    // 更新题目序号，确保在删除后左侧列表的题目编号正确
    private async void UpdateQuestionNumbers()
    {
        var exercises = await _poetryStorage.GetExerciseQuestionsAsync(null, 0, PageSize);

            for (int i = 0; i < exercises.Count; i++)
            {
                var exercise = exercises[i];
                if (exercise.question.Length > 8)
                {
                    exercise.question = exercise.question.Substring(0, 8) + "...";
                }
                exercise.question = $"（{(i + 1)}）{exercise.question}";
            }

            ExerciseQuestions = new ObservableCollection<Exercise>(exercises);
    }
    
    public ICommand RedoQuestionCommand { get; }


    private async void RedoQuestion()
    {
        if (_wrongNavigationService == null)
        {
            throw new InvalidOperationException("Navigation service is not initialized.");
        }
        Exercise e = await _poetryStorage.GetWrongQuestionByIdAsync(ExerciseQuestions[CurrentIndex].Id);
        _wrongNavigationService.NavigateTo(e);
    }

}
