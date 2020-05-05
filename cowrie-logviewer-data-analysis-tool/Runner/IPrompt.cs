namespace cowrie_logviewer_data_analysis_tool.Runner
{
    interface IPrompt<T>
    {
        string PromptInputMessage(T ignore);
        bool SetValue(T ignore, string input);
    }
}