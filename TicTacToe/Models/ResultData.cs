using System;

namespace TicTacToe.Models
{
    [Serializable]
    public class ResultData
    {
        public bool Success { get; set; }

        /// <summary>
        /// Contains description of Fail.
        /// </summary>
        public string FailReason { get; set; }
    }
}