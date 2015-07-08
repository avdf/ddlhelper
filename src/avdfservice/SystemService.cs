using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Topshelf;
using System.Timers;
using System.ComponentModel;
using MySql.Data.MySqlClient;
using Dapper;
using System.Collections.Generic;
using Serilog;

// Correct the ambiguous reference error to the Timer class
using Timer = System.Timers.Timer;

namespace ddlhelper
{
	/// <summary>
	/// System service for execution of MySQL DDL instructions that
	/// can't run from dynamic statements inside stored procedures.
	/// Author: Albert Victor d'Ângelo Foureaux
	/// </summary>
	public class SystemService
	{
		private Timer _timer;

		private BackgroundWorker _worker;

		Logger Log = new Logger();

		public SystemService()
		{


			_timer = new Timer (10000);
			_timer.Elapsed += Elapsed;
			_timer.Start ();

			_worker = new BackgroundWorker ();
			_worker.DoWork += DoWork;
		}

		/// <summary>
		/// Dos the work.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="eventArgs">Event arguments.</param>
		void DoWork (object sender, DoWorkEventArgs eventArgs)
		{
			string connectionString = "Server=localhost;Database=DBSGP;User ID=root;Password=13352*;Pooling=false";
			using (MySqlConnection connection = new MySqlConnection(connectionString)) 
			{
				try
				{
					connection.Open();

					// Consulta usando o Dapper
					IEnumerable<DDLScheduled> schedulerList;

					schedulerList = connection.Query<DDLScheduled>(@"SELECT COD_DDL_SCHEDULER AS CodDDLScheduler,
                                                                   DDL_SCRIPT AS DDLScript
                                                              FROM DBSGP.DDL_SCHEDULER
                                                             WHERE IND_EXECUTADO = 'N'
                                                             ORDER BY COD_DDL_SCHEDULER");

					foreach(DDLScheduled ddlScheduled in schedulerList)
					{
						// Execucao do comand DDL usando MySqlCommand
						string commandText = ddlScheduled.DDLScript;

						using(MySqlCommand command = new MySqlCommand(commandText, connection))
						{
							try
							{
								Log.Information("(PK:{0}) Inicio da execução ", ddlScheduled.CodDDLScheduler);
								command.ExecuteNonQuery();
								Log.Information("(PK:{0}) Comando DDL executado com sucesso.", ddlScheduled.CodDDLScheduler);
							}
							catch
							{
								Log.Error("(PK:{0}) Ocorreu uma exceção ao executar o comando DDL.", ddlScheduled.CodDDLScheduler);
							}
							finally
							{
								try
								{
									// Update usando novamente o Dapper
									string updateScheduler = @"UPDATE DBSGP.DDL_SCHEDULER
	                                                          SET IND_EXECUTADO = 'S'
	                                                        WHERE COD_DDL_SCHEDULER = " + ddlScheduled.CodDDLScheduler.ToString();

									connection.Execute(updateScheduler);
									Log.Information("(PK:{0}) Comando DDL marcado como executado.", ddlScheduled.CodDDLScheduler);
								}
								catch
								{
									Log.Error("(PK:{0}) Erro ao atualizar o registro como executado.", ddlScheduled.CodDDLScheduler);
								}
							}
						}
					}
				}
				catch (Exception e)
				{
					Console.WriteLine("==============================================================");
					Console.WriteLine(e.Message);
					Console.WriteLine("--------------------------------------------------------------");
				}
			}
		}

		public void Elapsed(object sender, ElapsedEventArgs e)  
		{
			Console.WriteLine ("Processando rotina do servico de auxilio a DDL...");
			if (!_worker.IsBusy)
				_worker.RunWorkerAsync ();
		}

		public void Start()
		{

		}

		public void Stop()
		{
		}
	}
}