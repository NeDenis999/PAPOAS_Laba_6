using System;
using System.Collections;
using System.Collections.Generic;
using PAPOAS.Code;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class UI : MonoBehaviour
{
    [SerializeField]
    private UIDocument _loginForm;
    
    [SerializeField]
    private UIDocument _clientForm;
    
    [SerializeField]
    private UIDocument _directorForm;
    
    [SerializeField]
    private UIDocument _detectiveForm;

    List<string> _choices = new List<string> { "Клиент", "Детектив", "Руководитель", "Сотрудник", "Аналитик" };
    private DropdownField _loginField;
    private List<TaskData> _underConsiderationTasks = new List<TaskData>();
    private List<TaskData> _inProcessTasks = new List<TaskData>();
    private List<TaskData> _rejectedTasks = new List<TaskData>();
    private List<TaskData> _completedTasks = new List<TaskData>();
    private List<UserData> _usersData = new List<UserData>();
    private UserData _user;
    private int _endNumber;

    private void Start()
    {
        if (PlayerPrefs.HasKey("BaseData"))
        {
            var baseDataString = PlayerPrefs.GetString("BaseData");
            var baseData = JsonUtility.FromJson<BaseData>(baseDataString);

            _underConsiderationTasks = baseData.UnderConsiderationTasks;
            _inProcessTasks = baseData.InProcessTasks;
            _rejectedTasks = baseData.RejectedTasks;
            _completedTasks = baseData.CompletedTasks;
            _usersData = baseData.UsersData;
            _endNumber = baseData.Number;
        }
        
        _detectiveForm.gameObject.SetActive(false);
        _clientForm.gameObject.SetActive(false);
        _directorForm.gameObject.SetActive(false);
        OpenLogin();
    }

    private void OnDestroy()
    {
        var baseData = new BaseData
        {
            UnderConsiderationTasks = _underConsiderationTasks,
            InProcessTasks = _inProcessTasks,
            RejectedTasks = _rejectedTasks,
            CompletedTasks = _completedTasks,
            UsersData = _usersData,
            Number = _endNumber
        };


        var baseDataString = JsonUtility.ToJson(baseData);
        PlayerPrefs.SetString("BaseData", baseDataString);
    }

    private void OnButtonClicked()
    {
        print("Клик");
        _loginForm.gameObject.SetActive(false);
        
        switch (_loginField.value)
        {
            case "Клиент":
                print("Клиент");
                _clientForm.gameObject.SetActive(true);
                
                var addTask = _clientForm.rootVisualElement.Q<Button>("Continue_Button");
                addTask.clickable.clicked += OnClientAddTaskButtonClicked;
                break;
            case "Детектив":
                _detectiveForm.gameObject.SetActive(true);
                
                var confirmTask = _detectiveForm.rootVisualElement.Q<Button>("Continue_Button");
                confirmTask.clickable.clicked += OnDetectiveConfirmButtonClicked;
                
                var fieldDetective = _detectiveForm.rootVisualElement.Q<DropdownField>("Role_DropdownField");
                fieldDetective.RegisterValueChangedCallback(OnDetectiveSelect);
                
                List<string> choices1 = new List<string>();

                foreach (var task in _inProcessTasks)
                {
                    choices1.Add($"Дело №{task.Number + 1}");
                }
                
                fieldDetective.choices = choices1;

                if (choices1.Count > 0)
                {
                    fieldDetective.value = choices1[0];
                    var labelTask1 = _detectiveForm.rootVisualElement.Q<Label>("TextTask_Label");
                    labelTask1.text = _inProcessTasks[0].Information;
                }
                break;
            case  "Руководитель":
                _directorForm.gameObject.SetActive(true);
                
                var giveTask = _directorForm.rootVisualElement.Q<Button>("Continue_Button");
                var removeask = _directorForm.rootVisualElement.Q<Button>("Remove_Button");
                giveTask.clicked += OnDirectorGiveTaskButtonClicked;
                removeask.clicked += OnDirectorRemoveTaskButtonClicked;
                
                var field = _directorForm.rootVisualElement.Q<DropdownField>("Role_DropdownField");
                field.RegisterValueChangedCallback(OnDirectorSelect);

                List<string> choices = new List<string>();

                foreach (var task in _underConsiderationTasks)
                {
                    choices.Add($"Дело №{task.Number + 1}");
                }
                
                field.choices = choices;
                field.value = choices[0];
                
                var labelTask = _directorForm.rootVisualElement.Q<Label>("TextTask_Label");
                labelTask.text = _underConsiderationTasks[0].Information;
                break;
            case "Сотрудник":
                break;
            case "Аналитик":
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void OnClientAddTaskButtonClicked()
    {
        TaskData task = new TaskData();
        var textField = _clientForm.rootVisualElement.Q<TextField>("Login_TextField");
        
        task.State = TaskState.UnderConsideration;
        task.Information = textField.value;
        task.User = _user;
        task.Number = _endNumber;
        _endNumber++;
        _underConsiderationTasks.Add(task);
        
        print(task.Number);
        print(task.State);
        print(task.Information);
        print(task.User);
        
        _clientForm.gameObject.SetActive(false);
        OpenLogin();
    }

    private void OnDetectiveConfirmButtonClicked()
    {
        var field = _detectiveForm.rootVisualElement.Q<DropdownField>("Role_DropdownField");
        var text_field = _detectiveForm.rootVisualElement.Q<TextField>("Login_TextField");
        print(field.index);

        var task = _inProcessTasks[field.index];
        _inProcessTasks.Remove(task);
        task.State = TaskState.InProcess;
        task.Result = text_field.value;
        _completedTasks.Add(task);
        
        _detectiveForm.gameObject.SetActive(false);
        OpenLogin();
    }

    private void OnDirectorGiveTaskButtonClicked()
    {
        var field = _directorForm.rootVisualElement.Q<DropdownField>("Role_DropdownField");
        print(field.index);

        var task = _underConsiderationTasks[field.index];
        _underConsiderationTasks.Remove(task);
        task.State = TaskState.InProcess;
        _inProcessTasks.Add(task);

        _directorForm.gameObject.SetActive(false);
        OpenLogin();
    }

    private void OnDirectorRemoveTaskButtonClicked()
    {
        var field = _directorForm.rootVisualElement.Q<DropdownField>("Role_DropdownField");
        print(field.index);

        var task = _underConsiderationTasks[field.index];
        _underConsiderationTasks.Remove(task);
        task.State = TaskState.Rejected;
        _rejectedTasks.Add(task);

        _directorForm.gameObject.SetActive(false);
        OpenLogin();
    }

    private void OnDetectiveSelect(ChangeEvent<string> evt)
    {
        var field = _detectiveForm.rootVisualElement.Q<DropdownField>("Role_DropdownField");
        
        var labelTask = _detectiveForm.rootVisualElement.Q<Label>("TextTask_Label");
        labelTask.text = _inProcessTasks[field.index].Information;
    }

    private void OnDirectorSelect(ChangeEvent<string> text)
    {
        var field = _directorForm.rootVisualElement.Q<DropdownField>("Role_DropdownField");
        
        var labelTask = _directorForm.rootVisualElement.Q<Label>("TextTask_Label");
        labelTask.text = _underConsiderationTasks[field.index].Information;
    }

    private void OpenLogin()
    {
        _loginForm.gameObject.SetActive(true);
        
        _loginField = _loginForm.rootVisualElement.Q<DropdownField>("Role_DropdownField");
        _loginField.choices = _choices;
        _loginField.value = _choices[0];
        
        var button = _loginForm.rootVisualElement.Q<Button>("Continue_Button");
        button.clickable.clicked += OnButtonClicked;
    }
}

public struct BaseData
{
    public List<TaskData> UnderConsiderationTasks;
    public List<TaskData> InProcessTasks;
    public List<TaskData> RejectedTasks;
    public List<TaskData> CompletedTasks;
    public List<UserData> UsersData;
    public int Number;
}
