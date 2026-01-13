using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnolageTests.Services
{
    public class DummyNotificationService : INotificationService
    {
        public void ShowNotification(string title, string message)
        {
            // ничего не делаем
        }

        public void ShowNotification(string title, string message, string testId)
        {
            
        }
    }

}
