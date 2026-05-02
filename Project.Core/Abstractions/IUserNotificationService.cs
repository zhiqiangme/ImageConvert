namespace ImageConvert.Core.Abstractions;

/// <summary>
/// 用户通知服务接口，由平台层（WPF）实现消息框弹出。
/// </summary>
public interface IUserNotificationService
{
    /// <summary>
    /// 显示信息提示框。
    /// </summary>
    /// <param name="title">提示框标题</param>
    /// <param name="message">提示内容</param>
    void ShowInfo(string title, string message);
}
