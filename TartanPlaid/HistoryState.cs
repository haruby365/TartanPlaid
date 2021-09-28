// © 2021 Jong-il Hong

using System;

namespace Haruby.TartanPlaid
{
    public class HistoryState
    {
        public HistoryState(string json)
        {
            Json = json ?? throw new ArgumentNullException(nameof(json));
        }

        public string Json { get; }
    }
}
