using Camunda.Api.Client;
using Camunda.Api.Client.History;
using Camunda.Api.Client.Message;
using Camunda.Api.Client.ProcessDefinition;
using Camunda.Api.Client.ProcessInstance;
using Camunda.Api.Client.User;
using Camunda.Api.Client.UserTask;
using ModeratorAplicationApp.Models;

namespace ModeratorAplicationApp.Util
{
    public class CamundaUtil
    {
        private const string camundaEngineUri = "http://localhost:8080/engine-rest";
        private static CamundaClient client = CamundaClient.Create(camundaEngineUri);
        private const string processKey = "ModeratorAplication";

        private const string applyMessage = "AcceptUserApplication";

        public static async Task<List<AplicationInfo>> GetAplications()
        {
            var historyList = await client.History.ProcessInstances.Query(new HistoricProcessInstanceQuery { ProcessDefinitionKey = processKey }).List();
            var aplications = historyList.OrderBy(p => p.StartTime)
                                     .Select(p => new AplicationInfo
                                     {
                                         StartTime = p.StartTime,
                                         EndTime = p.State == ProcessInstanceState.Completed ? p.EndTime : new DateTime?(),
                                         Ended = p.State == ProcessInstanceState.Completed,
                                         PID = p.Id
                                     })
                                     .ToList();

            var tasks = new List<Task>();
            foreach (var aplication in aplications)
            {
                tasks.Add(LoadInstanceVariables(aplication));
            }
            await Task.WhenAll(tasks);

            return aplications;
        }

        public static async Task<List<TaskInfo>> GetTasks(string username)
        {
            var userTasks = await client.UserTasks
                                        .Query(new TaskQuery
                                        {
                                            Assignee = username,
                                            ProcessDefinitionKey = processKey
                                        })
                                        .List();

            var list = userTasks.OrderBy(t => t.Created)
                                .Select(t => new TaskInfo
                                {
                                    TID = t.Id,
                                    TaskName = t.Name,
                                    TaskKey = t.TaskDefinitionKey,
                                    PID = t.ProcessInstanceId,
                                    StartTime = t.Created,
                                })
                                .ToList();

            var tasks = new List<Task>();
            foreach (var task in list)
            {
                tasks.Add(LoadTaskVariables(task));
            }
            await Task.WhenAll(tasks);
            return list;
        }

        public static async Task<string> StartAplicationProcess(int aplicationId, string aplicant, string email, string languageKnowledge, string motivationalLetter)
        {
            var parameters = new Dictionary<string, object>();
            parameters["ApplicationID"] = aplicationId;
            parameters["Aplicant"] = aplicant;
            parameters["Email"] = email;
            parameters["LanguageKnowledge"] = languageKnowledge;
            parameters["MotivationalLetter"] = motivationalLetter;
            var processInstanceId = await StartProcess(parameters);
            return processInstanceId;
        }

        private static async Task<string> StartProcess(Dictionary<string, object> processParameters)
        {
            var client = CamundaClient.Create(camundaEngineUri);
            StartProcessInstance parameters = new StartProcessInstance();
            foreach (var param in processParameters)
            {
                parameters.SetVariable(param.Key, param.Value);
            }
            var definition = client.ProcessDefinitions.ByKey(processKey);
            ProcessInstanceWithVariables processInstance = await definition.StartProcessInstance(parameters);
            return processInstance.Id;
        }

        public static async Task ApplyForQuestioning(string pid, string user)
        {
            var message = new CorrelationMessage()
            {
                ProcessInstanceId = pid,
                MessageName = applyMessage,
                All = true,
                BusinessKey = null
            };
            message.ProcessVariables.Set("Moderator", user);
            await client.Messages.DeliverMessage(message);
        }

        public static async Task PickTask(string taskId, string user)
        {
            await client.UserTasks[taskId].Claim(user);
        }

        public static async Task FinishTask(string taskId)
        {
            await client.UserTasks[taskId].Complete(new CompleteTask());
        }


        public static async Task AssignModerator(string taskId, string moderator)
        {
            var variables = new Dictionary<string, VariableValue>();
            variables["Moderator"] = VariableValue.FromObject(moderator);
            await client.UserTasks[taskId].Complete(new CompleteTask()
            {
                Variables = variables
            });
        }

        public static async Task FinishInitialReview(string taskId, bool passed)
        {
            var variables = new Dictionary<string, VariableValue>();
            variables["PassedInitial"] = VariableValue.FromObject(passed);
            variables["TimePassed"] = VariableValue.FromObject(false);
            await client.UserTasks[taskId].Complete(new CompleteTask()
            {
                Variables = variables
            });
        }

        public static async Task SendQuestionnaire(string taskId, string question1, string question2, string question3)
        {
            var variables = new Dictionary<string, VariableValue>
            {
                ["Question1"] = VariableValue.FromObject(question1),
                ["Question2"] = VariableValue.FromObject(question2),
                ["Question3"] = VariableValue.FromObject(question3)
            };
            await client.UserTasks[taskId].Complete(new CompleteTask()
            {
                Variables = variables
            });
        }

        public static async Task SendSolvedQuestionnaire(string taskId, string answer1, string answer2, string answer3)
        {
            var variables = new Dictionary<string, VariableValue>
            {
                ["Answer1"] = VariableValue.FromObject(answer1),
                ["Answer2"] = VariableValue.FromObject(answer2),
                ["Answer3"] = VariableValue.FromObject(answer3)
            };
            await client.UserTasks[taskId].Complete(new CompleteTask()
            {
                Variables = variables
            });
        }

        public static async Task FinishFinalReview(string taskId, bool passed)
        {
            var variables = new Dictionary<string, VariableValue>();
            variables["CandidateSelected"] = VariableValue.FromObject(passed);
            await client.UserTasks[taskId].Complete(new CompleteTask()
            {
                Variables = variables
            });
        }

        public static async Task<bool> IsUserInGroup(string user, string group)
        {
            var list = await client.Users
                                    .Query(new UserQuery
                                    {
                                        Id = user,
                                        MemberOfGroup = group
                                    })
                                    .List();
            return list.Count > 0;
        }

        private static async Task LoadTaskVariables(TaskInfo task)
        {
            var variables = await client.UserTasks[task.TID].Variables.GetAll();

            if (variables.TryGetValue("ApplicationID", out VariableValue value))
            {
                task.AplicationId = value.GetValue<int>();
            }

            if (variables.TryGetValue("Aplicant", out value))
            {
                task.Aplicant = value.GetValue<string>();
            }

            if (variables.TryGetValue("Email", out value))
            {
                task.Email = value.GetValue<string>();
            }

            if (variables.TryGetValue("LanguageKnowledge", out value))
            {
                task.LanguageKnowledge = value.GetValue<string>();
            }

            if (variables.TryGetValue("MotivationalLetter", out value))
            {
                task.MotivationalLetter = value.GetValue<string>();
            }

            if (variables.TryGetValue("Question1", out value))
            {
                task.Question1 = value.GetValue<string>();
            }

            if (variables.TryGetValue("Question2", out value))
            {
                task.Question2 = value.GetValue<string>();
            }

            if (variables.TryGetValue("Question3", out value))
            {
                task.Question3 = value.GetValue<string>();
            }

            if (variables.TryGetValue("Answer1", out value))
            {
                task.Answer1 = value.GetValue<string>();
            }

            if (variables.TryGetValue("Answer2", out value))
            {
                task.Answer2 = value.GetValue<string>();
            }

            if (variables.TryGetValue("Answer3", out value))
            {
                task.Answer3 = value.GetValue<string>();
            }

        }

        private static async Task LoadInstanceVariables(AplicationInfo aplication)
        {
            var list = await client.History.VariableInstances.Query(new HistoricVariableInstanceQuery { ProcessInstanceId = aplication.PID }).List();
            aplication.AplicationId = list.Where(v => v.Name == "ApplicationID")
                                    .Select(v => Convert.ToInt32(v.Value))
                                    .First();

            aplication.Aplicant = list.Where(v => v.Name == "Aplicant")
                                    .Select(v => (string)v.Value)
                                    .First();

            var moderator = list.Where(v => v.Name == "Moderator")
                                 .Select(v => v.Value as string)
                                 .FirstOrDefault();
            aplication.Moderator = moderator;

            var timePassed = list.Where(v => v.Name == "TimePassed")
                                  .Select(v => v.Value)
                                  .FirstOrDefault();

            aplication.Taken = list.Where(v => v.Name == "CandidateSelected")
                                    .Select(v => Convert.ToBoolean(v.Value))
                                    .FirstOrDefault(false);


            aplication.CanApplyForQuestioning = string.IsNullOrWhiteSpace(moderator) && ( timePassed != null && !Convert.ToBoolean(timePassed));
        }

        public static async Task<string> GetXmlDefinition()
        {
            var client = CamundaClient.Create(camundaEngineUri);
            var definition = client.ProcessDefinitions.ByKey(processKey);
            ProcessDefinitionDiagram diagram = await definition.GetXml();
            return diagram.Bpmn20Xml;
        }
    }
}
