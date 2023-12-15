using System;
using UnityEngine;

namespace PAPOAS.Code
{
    [Serializable]
    public struct TaskData
    {
        public int Number;
        public UserData User;
        public string Information;
        public string Result;
        public TaskState State;
    }

    public enum TaskState
    {
        [InspectorName("Находится на рассмотрении")]
        UnderConsideration = 0,
        [InspectorName("Выполняется")]
        InProcess = 1,
        [InspectorName("Закончен")]
        Completed = 2,
        [InspectorName("Отклонен")]
        Rejected = 3
    }
}