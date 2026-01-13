using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnolageTests.Services
{
    public interface INotificationService
    {
        void ShowNotification(string title, string message);
        void ShowNotification(string title, string message, string testId);
    }

    
}
