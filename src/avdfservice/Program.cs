using System;
using Topshelf;

namespace ddlhelper
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			HostFactory.Run(x =>                                 //1
			                {
				x.Service<SystemService>(s =>                        //2
				                     {
					s.ConstructUsing(name=> new SystemService());     //3
					s.WhenStarted(tc => tc.Start());              //4
					s.WhenStopped(tc => tc.Stop());               //5
				});
				x.RunAsLocalSystem();                            //6

				x.SetDescription("Sample Topshelf Host");        //7
				x.SetDisplayName("avdf service");                 
				x.SetServiceName("avdfservice");//9
			});
		}
	}
}
