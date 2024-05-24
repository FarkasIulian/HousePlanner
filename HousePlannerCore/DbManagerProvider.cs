using DBManager;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HousePlannerCore
{
    public static class DbManagerProvider
    {
        private static DbManagerService DbManager;

        public static void SetManager(IContainerProvider containerProvider)
        {
            if (DbManager == null)
                DbManager = containerProvider.Resolve<DbManagerService>();
        }

        public static DbManagerService GetDbManager()
        {
            return DbManager;
        }

    }
}
