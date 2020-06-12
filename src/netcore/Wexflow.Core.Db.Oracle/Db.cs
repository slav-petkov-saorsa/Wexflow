﻿using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wexflow.Core.Db.Oracle
{
    public sealed class Db : Core.Db.Db
    {
        private static readonly object padlock = new object();
        private static readonly string dateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";

        private static string connectionString;

        public Db(string connectionString) : base(connectionString)
        {
            Db.connectionString = connectionString;
            var helper = new Helper(connectionString);
            helper.CreateTableIfNotExists(Core.Db.Entry.DocumentName, Entry.TableStruct);
            helper.CreateTableIfNotExists(Core.Db.HistoryEntry.DocumentName, HistoryEntry.TableStruct);
            helper.CreateTableIfNotExists(Core.Db.StatusCount.DocumentName, StatusCount.TableStruct);
            helper.CreateTableIfNotExists(Core.Db.User.DocumentName, User.TableStruct);
            helper.CreateTableIfNotExists(Core.Db.UserWorkflow.DocumentName, UserWorkflow.TableStruct);
            helper.CreateTableIfNotExists(Core.Db.Workflow.DocumentName, Workflow.TableStruct);
            helper.CreateTableIfNotExists(Core.Db.Version.DocumentName, Version.TableStruct);
            helper.CreateTableIfNotExists(Core.Db.Record.DocumentName, Record.TableStruct);
            helper.CreateTableIfNotExists(Core.Db.Notification.DocumentName, Notification.TableStruct);
            helper.CreateTableIfNotExists(Core.Db.Approver.DocumentName, Approver.TableStruct);
        }

        public override void Init()
        {
            // StatusCount
            ClearStatusCount();

            var statusCount = new StatusCount
            {
                PendingCount = 0,
                RunningCount = 0,
                DoneCount = 0,
                FailedCount = 0,
                WarningCount = 0,
                DisabledCount = 0,
                StoppedCount = 0
            };

            using (var conn = new OracleConnection(connectionString))
            {
                conn.Open();

                using (var command = new OracleCommand("INSERT INTO " + Core.Db.StatusCount.DocumentName + "("
                    + StatusCount.ColumnName_PendingCount + ", "
                    + StatusCount.ColumnName_RunningCount + ", "
                    + StatusCount.ColumnName_DoneCount + ", "
                    + StatusCount.ColumnName_FailedCount + ", "
                    + StatusCount.ColumnName_WarningCount + ", "
                    + StatusCount.ColumnName_DisabledCount + ", "
                    + StatusCount.ColumnName_StoppedCount + ", "
                    + StatusCount.ColumnName_RejectedCount + ") VALUES("
                    + statusCount.PendingCount + ", "
                    + statusCount.RunningCount + ", "
                    + statusCount.DoneCount + ", "
                    + statusCount.FailedCount + ", "
                    + statusCount.WarningCount + ", "
                    + statusCount.DisabledCount + ", "
                    + statusCount.StoppedCount + ", "
                    + statusCount.RejectedCount + ")"
                    , conn))
                {
                    command.ExecuteNonQuery();
                }
            }

            // Entries
            ClearEntries();

            // Insert default user if necessary
            using (var conn = new OracleConnection(connectionString))
            {
                conn.Open();

                using (var command = new OracleCommand("SELECT COUNT(*) FROM " + Core.Db.User.DocumentName, conn))
                {
                    var usersCount = Convert.ToInt64((decimal)command.ExecuteScalar());

                    if (usersCount == 0)
                    {
                        InsertDefaultUser();
                    }
                }
            }
        }

        public override bool CheckUserWorkflow(string userId, string workflowId)
        {
            lock (padlock)
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("SELECT COUNT(*) FROM " + Core.Db.UserWorkflow.DocumentName
                        + " WHERE " + UserWorkflow.ColumnName_UserId + "=" + int.Parse(userId)
                        + " AND " + UserWorkflow.ColumnName_WorkflowId + "=" + int.Parse(workflowId)
                        , conn))
                    {
                        var count = Convert.ToInt64((decimal)command.ExecuteScalar());

                        return count > 0;
                    }

                }
            }
        }

        public override void ClearEntries()
        {
            lock (padlock)
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("DELETE FROM " + Core.Db.Entry.DocumentName, conn))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public override void ClearStatusCount()
        {
            lock (padlock)
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("DELETE FROM " + Core.Db.StatusCount.DocumentName, conn))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public override void DeleteUser(string username, string password)
        {
            lock (padlock)
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("DELETE FROM " + Core.Db.User.DocumentName
                        + " WHERE " + User.ColumnName_Username + " = '" + username + "'"
                        + " AND " + User.ColumnName_Password + " = '" + password + "'"
                        , conn))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public override void DeleteUserWorkflowRelationsByUserId(string userId)
        {
            lock (padlock)
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("DELETE FROM " + Core.Db.UserWorkflow.DocumentName
                        + " WHERE " + UserWorkflow.ColumnName_UserId + " = " + int.Parse(userId), conn))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public override void DeleteUserWorkflowRelationsByWorkflowId(string workflowDbId)
        {
            lock (padlock)
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("DELETE FROM " + Core.Db.UserWorkflow.DocumentName
                        + " WHERE " + UserWorkflow.ColumnName_WorkflowId + " = " + int.Parse(workflowDbId), conn))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public override void DeleteWorkflow(string id)
        {
            lock (padlock)
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("DELETE FROM " + Core.Db.Workflow.DocumentName
                        + " WHERE " + Workflow.ColumnName_Id + " = " + int.Parse(id), conn))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public override void DeleteWorkflows(string[] ids)
        {
            lock (padlock)
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    var builder = new StringBuilder("(");

                    for (int i = 0; i < ids.Length; i++)
                    {
                        var id = ids[i];
                        builder.Append(id);
                        if (i < ids.Length - 1)
                        {
                            builder.Append(", ");
                        }
                        else
                        {
                            builder.Append(")");
                        }
                    }

                    using (var command = new OracleCommand("DELETE FROM " + Core.Db.Workflow.DocumentName
                        + " WHERE " + Workflow.ColumnName_Id + " IN " + builder.ToString(), conn))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public override IEnumerable<Core.Db.User> GetAdministrators(string keyword, UserOrderBy uo)
        {
            lock (padlock)
            {
                List<User> admins = new List<User>();

                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("SELECT " + User.ColumnName_Id + ", "
                        + User.ColumnName_Username + ", "
                        + User.ColumnName_Password + ", "
                        + User.ColumnName_Email + ", "
                        + User.ColumnName_UserProfile + ", "
                        + User.ColumnName_CreatedOn + ", "
                        + User.ColumnName_ModifiedOn
                        + " FROM " + Core.Db.User.DocumentName
                        + " WHERE " + "(LOWER(" + User.ColumnName_Username + ")" + " LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%'"
                        + " AND " + User.ColumnName_UserProfile + " = " + (int)UserProfile.Administrator + ")"
                        + " ORDER BY " + User.ColumnName_Username + (uo == UserOrderBy.UsernameAscending ? " ASC" : " DESC")
                        , conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var admin = new User
                                {
                                    Id = Convert.ToInt64((decimal)reader[User.ColumnName_Id]),
                                    Username = (string)reader[User.ColumnName_Username],
                                    Password = (string)reader[User.ColumnName_Password],
                                    Email = reader[User.ColumnName_Email] == DBNull.Value ? string.Empty : (string)reader[User.ColumnName_Email],
                                    UserProfile = (UserProfile)(Convert.ToInt32((decimal)reader[User.ColumnName_UserProfile])),
                                    CreatedOn = (DateTime)reader[User.ColumnName_CreatedOn],
                                    ModifiedOn = reader[User.ColumnName_ModifiedOn] == DBNull.Value ? DateTime.MinValue : (DateTime)reader[User.ColumnName_ModifiedOn]
                                };

                                admins.Add(admin);
                            }
                        }
                    }
                }

                return admins;
            }
        }

        public override IEnumerable<Core.Db.Entry> GetEntries()
        {
            lock (padlock)
            {
                List<Entry> entries = new List<Entry>();

                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("SELECT "
                        + Entry.ColumnName_Id + ", "
                        + Entry.ColumnName_Name + ", "
                        + Entry.ColumnName_Description + ", "
                        + Entry.ColumnName_LaunchType + ", "
                        + Entry.ColumnName_Status + ", "
                        + Entry.ColumnName_StatusDate + ", "
                        + Entry.ColumnName_WorkflowId + ", "
                        + Entry.ColumnName_JobId
                        + " FROM " + Core.Db.Entry.DocumentName, conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {

                            while (reader.Read())
                            {
                                var entry = new Entry
                                {
                                    Id = Convert.ToInt64((decimal)reader[Entry.ColumnName_Id]),
                                    Name = reader[Entry.ColumnName_Name] == DBNull.Value ? string.Empty : (string)reader[Entry.ColumnName_Name],
                                    Description = reader[Entry.ColumnName_Description] == DBNull.Value ? string.Empty : (string)reader[Entry.ColumnName_Description],
                                    LaunchType = (LaunchType)(Convert.ToInt32((decimal)reader[Entry.ColumnName_LaunchType])),
                                    Status = (Status)(Convert.ToInt32((decimal)reader[Entry.ColumnName_Status])),
                                    StatusDate = (DateTime)reader[Entry.ColumnName_StatusDate],
                                    WorkflowId = (Convert.ToInt32((decimal)reader[Entry.ColumnName_WorkflowId])),
                                    JobId = reader[Entry.ColumnName_JobId] == DBNull.Value ? string.Empty : (string)reader[Entry.ColumnName_JobId]
                                };

                                entries.Add(entry);
                            }
                        }
                    }
                }

                return entries;
            }
        }

        public override IEnumerable<Core.Db.Entry> GetEntries(string keyword, DateTime from, DateTime to, int page, int entriesCount, EntryOrderBy eo)
        {
            lock (padlock)
            {
                List<Entry> entries = new List<Entry>();

                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    var sqlBuilder = new StringBuilder("SELECT "
                        + Entry.ColumnName_Id + ", "
                        + Entry.ColumnName_Name + ", "
                        + Entry.ColumnName_Description + ", "
                        + Entry.ColumnName_LaunchType + ", "
                        + Entry.ColumnName_Status + ", "
                        + Entry.ColumnName_StatusDate + ", "
                        + Entry.ColumnName_WorkflowId + ", "
                        + Entry.ColumnName_JobId
                        + " FROM " + Core.Db.Entry.DocumentName
                        + " WHERE " + "(LOWER(" + Entry.ColumnName_Name + ") LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%'"
                        + " OR " + "LOWER(" + Entry.ColumnName_Description + ") LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%')"
                        + " AND (" + Entry.ColumnName_StatusDate + " BETWEEN TO_TIMESTAMP('" + from.ToString(dateTimeFormat) + "', 'YYYY-MM-DD HH24:MI:SS.FF') AND TO_TIMESTAMP('" + to.ToString(dateTimeFormat) + "', 'YYYY-MM-DD HH24:MI:SS.FF'))"
                        + " ORDER BY ");

                    switch (eo)
                    {
                        case EntryOrderBy.StatusDateAscending:

                            sqlBuilder.Append(Entry.ColumnName_StatusDate).Append(" ASC");
                            break;

                        case EntryOrderBy.StatusDateDescending:

                            sqlBuilder.Append(Entry.ColumnName_StatusDate).Append(" DESC");
                            break;

                        case EntryOrderBy.WorkflowIdAscending:

                            sqlBuilder.Append(Entry.ColumnName_WorkflowId).Append(" ASC");
                            break;

                        case EntryOrderBy.WorkflowIdDescending:

                            sqlBuilder.Append(Entry.ColumnName_WorkflowId).Append(" DESC");
                            break;

                        case EntryOrderBy.NameAscending:

                            sqlBuilder.Append(Entry.ColumnName_Name).Append(" ASC");
                            break;

                        case EntryOrderBy.NameDescending:

                            sqlBuilder.Append(Entry.ColumnName_Name).Append(" DESC");
                            break;

                        case EntryOrderBy.LaunchTypeAscending:

                            sqlBuilder.Append(Entry.ColumnName_LaunchType).Append(" ASC");
                            break;

                        case EntryOrderBy.LaunchTypeDescending:

                            sqlBuilder.Append(Entry.ColumnName_LaunchType).Append(" DESC");
                            break;

                        case EntryOrderBy.DescriptionAscending:

                            sqlBuilder.Append(Entry.ColumnName_Description).Append(" ASC");
                            break;

                        case EntryOrderBy.DescriptionDescending:

                            sqlBuilder.Append(Entry.ColumnName_Description).Append(" DESC");
                            break;

                        case EntryOrderBy.StatusAscending:

                            sqlBuilder.Append(Entry.ColumnName_Status).Append(" ASC");
                            break;

                        case EntryOrderBy.StatusDescending:

                            sqlBuilder.Append(Entry.ColumnName_Status).Append(" DESC");
                            break;
                    }

                    sqlBuilder.Append(" OFFSET ").Append((page - 1) * entriesCount).Append(" ROWS FETCH NEXT ").Append(entriesCount).Append(" ROWS ONLY");

                    using (var command = new OracleCommand(sqlBuilder.ToString(), conn))
                    {

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var entry = new Entry
                                {
                                    Id = Convert.ToInt64((decimal)reader[Entry.ColumnName_Id]),
                                    Name = reader[Entry.ColumnName_Name] == DBNull.Value ? string.Empty : (string)reader[Entry.ColumnName_Name],
                                    Description = reader[Entry.ColumnName_Description] == DBNull.Value ? string.Empty : (string)reader[Entry.ColumnName_Description],
                                    LaunchType = (LaunchType)(Convert.ToInt32((decimal)reader[Entry.ColumnName_LaunchType])),
                                    Status = (Status)(Convert.ToInt32((decimal)reader[Entry.ColumnName_Status])),
                                    StatusDate = (DateTime)reader[Entry.ColumnName_StatusDate],
                                    WorkflowId = (Convert.ToInt32((decimal)reader[Entry.ColumnName_WorkflowId])),
                                    JobId = reader[Entry.ColumnName_JobId] == DBNull.Value ? string.Empty : (string)reader[Entry.ColumnName_JobId]
                                };

                                entries.Add(entry);
                            }
                        }

                    }
                }

                return entries;
            }
        }

        public override long GetEntriesCount(string keyword, DateTime from, DateTime to)
        {
            lock (padlock)
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("SELECT COUNT(*)"
                        + " FROM " + Core.Db.Entry.DocumentName
                        + " WHERE " + "(LOWER(" + Entry.ColumnName_Name + ") LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%'"
                        + " OR " + "LOWER(" + Entry.ColumnName_Description + ") LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%')"
                        + " AND (" + Entry.ColumnName_StatusDate + " BETWEEN TO_TIMESTAMP('" + from.ToString(dateTimeFormat) + "', 'YYYY-MM-DD HH24:MI:SS.FF') AND TO_TIMESTAMP('" + to.ToString(dateTimeFormat) + "', 'YYYY-MM-DD HH24:MI:SS.FF'))", conn))
                    {
                        var count = (decimal)command.ExecuteScalar();

                        return Convert.ToInt64(count);
                    }
                }
            }
        }

        public override Core.Db.Entry GetEntry(int workflowId)
        {
            lock (padlock)
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("SELECT "
                        + Entry.ColumnName_Id + ", "
                        + Entry.ColumnName_Name + ", "
                        + Entry.ColumnName_Description + ", "
                        + Entry.ColumnName_LaunchType + ", "
                        + Entry.ColumnName_Status + ", "
                        + Entry.ColumnName_StatusDate + ", "
                        + Entry.ColumnName_WorkflowId + ", "
                        + Entry.ColumnName_JobId
                        + " FROM " + Core.Db.Entry.DocumentName
                        + " WHERE " + Entry.ColumnName_WorkflowId + " = " + workflowId, conn))
                    {

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var entry = new Entry
                                {
                                    Id = Convert.ToInt64((decimal)reader[Entry.ColumnName_Id]),
                                    Name = reader[Entry.ColumnName_Name] == DBNull.Value ? string.Empty : (string)reader[Entry.ColumnName_Name],
                                    Description = reader[Entry.ColumnName_Description] == DBNull.Value ? string.Empty : (string)reader[Entry.ColumnName_Description],
                                    LaunchType = (LaunchType)(Convert.ToInt32((decimal)reader[Entry.ColumnName_LaunchType])),
                                    Status = (Status)(Convert.ToInt32((decimal)reader[Entry.ColumnName_Status])),
                                    StatusDate = (DateTime)reader[Entry.ColumnName_StatusDate],
                                    WorkflowId = (Convert.ToInt32((decimal)reader[Entry.ColumnName_WorkflowId])),
                                    JobId = reader[Entry.ColumnName_JobId] == DBNull.Value ? string.Empty : (string)reader[Entry.ColumnName_JobId]
                                };

                                return entry;
                            }
                        }
                    }

                }

                return null;
            }
        }

        public override Core.Db.Entry GetEntry(int workflowId, Guid jobId)
        {
            lock (padlock)
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("SELECT "
                        + Entry.ColumnName_Id + ", "
                        + Entry.ColumnName_Name + ", "
                        + Entry.ColumnName_Description + ", "
                        + Entry.ColumnName_LaunchType + ", "
                        + Entry.ColumnName_Status + ", "
                        + Entry.ColumnName_StatusDate + ", "
                        + Entry.ColumnName_WorkflowId + ", "
                        + Entry.ColumnName_JobId
                        + " FROM " + Core.Db.Entry.DocumentName
                        + " WHERE (" + Entry.ColumnName_WorkflowId + " = " + workflowId
                        + " AND " + Entry.ColumnName_JobId + " = '" + jobId.ToString() + "')", conn))
                    {

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var entry = new Entry
                                {
                                    Id = Convert.ToInt64((decimal)reader[Entry.ColumnName_Id]),
                                    Name = reader[Entry.ColumnName_Name] == DBNull.Value ? string.Empty : (string)reader[Entry.ColumnName_Name],
                                    Description = reader[Entry.ColumnName_Description] == DBNull.Value ? string.Empty : (string)reader[Entry.ColumnName_Description],
                                    LaunchType = (LaunchType)(Convert.ToInt32((decimal)reader[Entry.ColumnName_LaunchType])),
                                    Status = (Status)(Convert.ToInt32((decimal)reader[Entry.ColumnName_Status])),
                                    StatusDate = (DateTime)reader[Entry.ColumnName_StatusDate],
                                    WorkflowId = (Convert.ToInt32((decimal)reader[Entry.ColumnName_WorkflowId])),
                                    JobId = reader[Entry.ColumnName_JobId] == DBNull.Value ? string.Empty : (string)reader[Entry.ColumnName_JobId]
                                };

                                return entry;
                            }
                        }
                    }

                }

                return null;
            }
        }

        public override DateTime GetEntryStatusDateMax()
        {
            lock (padlock)
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("SELECT " + Entry.ColumnName_StatusDate
                        + " FROM " + Core.Db.Entry.DocumentName
                        + " WHERE rownum = 1"
                        + " ORDER BY " + Entry.ColumnName_StatusDate + " DESC", conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var statusDate = (DateTime)reader[Entry.ColumnName_StatusDate];

                                return statusDate;
                            }
                        }
                    }
                }

                return DateTime.Now;
            }
        }

        public override DateTime GetEntryStatusDateMin()
        {
            lock (padlock)
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("SELECT " + Entry.ColumnName_StatusDate
                        + " FROM " + Core.Db.Entry.DocumentName
                        + " WHERE rownum = 1"
                        + " ORDER BY " + Entry.ColumnName_StatusDate + " ASC", conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var statusDate = (DateTime)reader[Entry.ColumnName_StatusDate];

                                return statusDate;
                            }
                        }
                    }
                }

                return DateTime.Now;
            }
        }

        public override IEnumerable<Core.Db.HistoryEntry> GetHistoryEntries()
        {
            lock (padlock)
            {
                List<HistoryEntry> entries = new List<HistoryEntry>();

                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("SELECT "
                        + HistoryEntry.ColumnName_Id + ", "
                        + HistoryEntry.ColumnName_Name + ", "
                        + HistoryEntry.ColumnName_Description + ", "
                        + HistoryEntry.ColumnName_LaunchType + ", "
                        + HistoryEntry.ColumnName_Status + ", "
                        + HistoryEntry.ColumnName_StatusDate + ", "
                        + HistoryEntry.ColumnName_WorkflowId
                        + " FROM " + Core.Db.HistoryEntry.DocumentName, conn))
                    {

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var entry = new HistoryEntry
                                {
                                    Id = Convert.ToInt64((decimal)reader[HistoryEntry.ColumnName_Id]),
                                    Name = reader[HistoryEntry.ColumnName_Name] == DBNull.Value ? string.Empty : (string)reader[HistoryEntry.ColumnName_Name],
                                    Description = reader[HistoryEntry.ColumnName_Description] == DBNull.Value ? string.Empty : (string)reader[HistoryEntry.ColumnName_Description],
                                    LaunchType = (LaunchType)(Convert.ToInt32((decimal)reader[HistoryEntry.ColumnName_LaunchType])),
                                    Status = (Status)(Convert.ToInt32((decimal)reader[HistoryEntry.ColumnName_Status])),
                                    StatusDate = (DateTime)reader[HistoryEntry.ColumnName_StatusDate],
                                    WorkflowId = (Convert.ToInt32((decimal)reader[HistoryEntry.ColumnName_WorkflowId])),
                                };

                                entries.Add(entry);
                            }
                        }

                    }
                }

                return entries;
            }
        }

        public override IEnumerable<Core.Db.HistoryEntry> GetHistoryEntries(string keyword)
        {
            lock (padlock)
            {
                List<HistoryEntry> entries = new List<HistoryEntry>();

                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("SELECT "
                        + HistoryEntry.ColumnName_Id + ", "
                        + HistoryEntry.ColumnName_Name + ", "
                        + HistoryEntry.ColumnName_Description + ", "
                        + HistoryEntry.ColumnName_LaunchType + ", "
                        + HistoryEntry.ColumnName_Status + ", "
                        + HistoryEntry.ColumnName_StatusDate + ", "
                        + HistoryEntry.ColumnName_WorkflowId
                        + " FROM " + Core.Db.HistoryEntry.DocumentName
                        + " WHERE " + "LOWER(" + HistoryEntry.ColumnName_Name + ") LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%'"
                        + " OR " + "LOWER(" + HistoryEntry.ColumnName_Description + ") LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%'", conn))
                    {

                        using (var reader = command.ExecuteReader())
                        {

                            while (reader.Read())
                            {
                                var entry = new HistoryEntry
                                {
                                    Id = Convert.ToInt64((decimal)reader[HistoryEntry.ColumnName_Id]),
                                    Name = reader[HistoryEntry.ColumnName_Name] == DBNull.Value ? string.Empty : (string)reader[HistoryEntry.ColumnName_Name],
                                    Description = reader[HistoryEntry.ColumnName_Description] == DBNull.Value ? string.Empty : (string)reader[HistoryEntry.ColumnName_Description],
                                    LaunchType = (LaunchType)(Convert.ToInt32((decimal)reader[HistoryEntry.ColumnName_LaunchType])),
                                    Status = (Status)(Convert.ToInt32((decimal)reader[HistoryEntry.ColumnName_Status])),
                                    StatusDate = (DateTime)reader[HistoryEntry.ColumnName_StatusDate],
                                    WorkflowId = (Convert.ToInt32((decimal)reader[HistoryEntry.ColumnName_WorkflowId])),
                                };

                                entries.Add(entry);
                            }
                        }

                    }

                }

                return entries;
            }
        }

        public override IEnumerable<Core.Db.HistoryEntry> GetHistoryEntries(string keyword, int page, int entriesCount)
        {
            lock (padlock)
            {
                List<HistoryEntry> entries = new List<HistoryEntry>();

                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("SELECT "
                        + HistoryEntry.ColumnName_Id + ", "
                        + HistoryEntry.ColumnName_Name + ", "
                        + HistoryEntry.ColumnName_Description + ", "
                        + HistoryEntry.ColumnName_LaunchType + ", "
                        + HistoryEntry.ColumnName_Status + ", "
                        + HistoryEntry.ColumnName_StatusDate + ", "
                        + HistoryEntry.ColumnName_WorkflowId
                        + " FROM " + Core.Db.HistoryEntry.DocumentName
                        + " WHERE " + "LOWER(" + HistoryEntry.ColumnName_Name + ") LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%'"
                        + " OR " + "LOWER(" + HistoryEntry.ColumnName_Description + ") LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%'"
                        + " OFFSET " + (page - 1) * entriesCount + " ROWS FETCH NEXT " + entriesCount + " ROWS ONLY"
                        , conn))
                    {

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var entry = new HistoryEntry
                                {
                                    Id = Convert.ToInt64((decimal)reader[HistoryEntry.ColumnName_Id]),
                                    Name = reader[HistoryEntry.ColumnName_Name] == DBNull.Value ? string.Empty : (string)reader[HistoryEntry.ColumnName_Name],
                                    Description = reader[HistoryEntry.ColumnName_Description] == DBNull.Value ? string.Empty : (string)reader[HistoryEntry.ColumnName_Description],
                                    LaunchType = (LaunchType)(Convert.ToInt32((decimal)reader[HistoryEntry.ColumnName_LaunchType])),
                                    Status = (Status)(Convert.ToInt32((decimal)reader[HistoryEntry.ColumnName_Status])),
                                    StatusDate = (DateTime)reader[HistoryEntry.ColumnName_StatusDate],
                                    WorkflowId = (Convert.ToInt32((decimal)reader[HistoryEntry.ColumnName_WorkflowId])),
                                };

                                entries.Add(entry);
                            }
                        }

                    }
                }
                return entries;
            }
        }

        public override IEnumerable<Core.Db.HistoryEntry> GetHistoryEntries(string keyword, DateTime from, DateTime to, int page, int entriesCount, EntryOrderBy heo)
        {
            lock (padlock)
            {
                List<HistoryEntry> entries = new List<HistoryEntry>();

                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    var sqlBuilder = new StringBuilder("SELECT "
                        + HistoryEntry.ColumnName_Id + ", "
                        + HistoryEntry.ColumnName_Name + ", "
                        + HistoryEntry.ColumnName_Description + ", "
                        + HistoryEntry.ColumnName_LaunchType + ", "
                        + HistoryEntry.ColumnName_Status + ", "
                        + HistoryEntry.ColumnName_StatusDate + ", "
                        + HistoryEntry.ColumnName_WorkflowId
                        + " FROM " + Core.Db.HistoryEntry.DocumentName
                        + " WHERE " + "(LOWER(" + HistoryEntry.ColumnName_Name + ") LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%'"
                        + " OR " + "LOWER(" + HistoryEntry.ColumnName_Description + ") LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%')"
                        + " AND (" + HistoryEntry.ColumnName_StatusDate + " BETWEEN TO_TIMESTAMP('" + from.ToString(dateTimeFormat) + "', 'YYYY-MM-DD HH24:MI:SS.FF') AND TO_TIMESTAMP('" + to.ToString(dateTimeFormat) + "', 'YYYY-MM-DD HH24:MI:SS.FF'))"
                        + " ORDER BY ");

                    switch (heo)
                    {
                        case EntryOrderBy.StatusDateAscending:

                            sqlBuilder.Append(HistoryEntry.ColumnName_StatusDate).Append(" ASC");
                            break;

                        case EntryOrderBy.StatusDateDescending:

                            sqlBuilder.Append(HistoryEntry.ColumnName_StatusDate).Append(" DESC");
                            break;

                        case EntryOrderBy.WorkflowIdAscending:

                            sqlBuilder.Append(HistoryEntry.ColumnName_WorkflowId).Append(" ASC");
                            break;

                        case EntryOrderBy.WorkflowIdDescending:

                            sqlBuilder.Append(HistoryEntry.ColumnName_WorkflowId).Append(" DESC");
                            break;

                        case EntryOrderBy.NameAscending:

                            sqlBuilder.Append(HistoryEntry.ColumnName_Name).Append(" ASC");
                            break;

                        case EntryOrderBy.NameDescending:

                            sqlBuilder.Append(HistoryEntry.ColumnName_Name).Append(" DESC");
                            break;

                        case EntryOrderBy.LaunchTypeAscending:

                            sqlBuilder.Append(HistoryEntry.ColumnName_LaunchType).Append(" ASC");
                            break;

                        case EntryOrderBy.LaunchTypeDescending:

                            sqlBuilder.Append(HistoryEntry.ColumnName_LaunchType).Append(" DESC");
                            break;

                        case EntryOrderBy.DescriptionAscending:

                            sqlBuilder.Append(HistoryEntry.ColumnName_Description).Append(" ASC");
                            break;

                        case EntryOrderBy.DescriptionDescending:

                            sqlBuilder.Append(HistoryEntry.ColumnName_Description).Append(" DESC");
                            break;

                        case EntryOrderBy.StatusAscending:

                            sqlBuilder.Append(HistoryEntry.ColumnName_Status).Append(" ASC");
                            break;

                        case EntryOrderBy.StatusDescending:

                            sqlBuilder.Append(HistoryEntry.ColumnName_Status).Append(" DESC");
                            break;
                    }

                    sqlBuilder.Append(" OFFSET ").Append((page - 1) * entriesCount).Append(" ROWS FETCH NEXT ").Append(entriesCount).Append(" ROWS ONLY");

                    using (var command = new OracleCommand(sqlBuilder.ToString(), conn))
                    {

                        using (var reader = command.ExecuteReader())
                        {

                            while (reader.Read())
                            {
                                var entry = new HistoryEntry
                                {
                                    Id = Convert.ToInt64((decimal)reader[HistoryEntry.ColumnName_Id]),
                                    Name = reader[HistoryEntry.ColumnName_Name] == DBNull.Value ? string.Empty : (string)reader[HistoryEntry.ColumnName_Name],
                                    Description = reader[HistoryEntry.ColumnName_Description] == DBNull.Value ? string.Empty : (string)reader[HistoryEntry.ColumnName_Description],
                                    LaunchType = (LaunchType)(Convert.ToInt32((decimal)reader[HistoryEntry.ColumnName_LaunchType])),
                                    Status = (Status)(Convert.ToInt32((decimal)reader[HistoryEntry.ColumnName_Status])),
                                    StatusDate = (DateTime)reader[HistoryEntry.ColumnName_StatusDate],
                                    WorkflowId = (Convert.ToInt32((decimal)reader[HistoryEntry.ColumnName_WorkflowId])),
                                };

                                entries.Add(entry);
                            }
                        }
                    }
                }

                return entries;
            }
        }

        public override long GetHistoryEntriesCount(string keyword)
        {
            lock (padlock)
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("SELECT COUNT(*)"
                        + " FROM " + Core.Db.HistoryEntry.DocumentName
                        + " WHERE " + "LOWER(" + HistoryEntry.ColumnName_Name + ") LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%'"
                        + " OR " + "LOWER(" + HistoryEntry.ColumnName_Description + ") LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%'", conn))
                    {
                        var count = (decimal)command.ExecuteScalar();

                        return Convert.ToInt64(count);
                    }
                }
            }
        }

        public override long GetHistoryEntriesCount(string keyword, DateTime from, DateTime to)
        {
            lock (padlock)
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("SELECT COUNT(*)"
                        + " FROM " + Core.Db.HistoryEntry.DocumentName
                        + " WHERE " + "(LOWER(" + HistoryEntry.ColumnName_Name + ") LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%'"
                        + " OR " + "LOWER(" + HistoryEntry.ColumnName_Description + ") LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%')"
                        + " AND (" + HistoryEntry.ColumnName_StatusDate + " BETWEEN TO_TIMESTAMP('" + from.ToString(dateTimeFormat) + "', 'YYYY-MM-DD HH24:MI:SS.FF') AND TO_TIMESTAMP('" + to.ToString(dateTimeFormat) + "', 'YYYY-MM-DD HH24:MI:SS.FF'))", conn))
                    {
                        var count = (decimal)command.ExecuteScalar();

                        return Convert.ToInt64(count);
                    }
                }
            }
        }

        public override DateTime GetHistoryEntryStatusDateMax()
        {
            lock (padlock)
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("SELECT " + HistoryEntry.ColumnName_StatusDate
                        + " FROM " + Core.Db.HistoryEntry.DocumentName
                        + " WHERE rownum = 1"
                        + " ORDER BY " + HistoryEntry.ColumnName_StatusDate + " DESC", conn))
                    {

                        using (var reader = command.ExecuteReader())
                        {

                            if (reader.Read())
                            {
                                var statusDate = (DateTime)reader[HistoryEntry.ColumnName_StatusDate];

                                return statusDate;
                            }
                        }
                    }
                }

                return DateTime.Now;
            }
        }

        public override DateTime GetHistoryEntryStatusDateMin()
        {
            lock (padlock)
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("SELECT " + HistoryEntry.ColumnName_StatusDate
                        + " FROM " + Core.Db.HistoryEntry.DocumentName
                        + " WHERE rownum = 1"
                        + " ORDER BY " + HistoryEntry.ColumnName_StatusDate + " ASC", conn))
                    {

                        using (var reader = command.ExecuteReader())
                        {

                            if (reader.Read())
                            {
                                var statusDate = (DateTime)reader[HistoryEntry.ColumnName_StatusDate];

                                return statusDate;
                            }
                        }
                    }
                }

                return DateTime.Now;
            }
        }

        public override string GetPassword(string username)
        {
            lock (padlock)
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("SELECT " + User.ColumnName_Password
                        + " FROM " + Core.Db.User.DocumentName
                        + " WHERE " + User.ColumnName_Username + " = '" + (username ?? "").Replace("'", "''") + "'"
                        , conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var password = (string)reader[User.ColumnName_Password];

                                return password;
                            }
                        }
                    }
                }

                return null;
            }
        }

        public override Core.Db.StatusCount GetStatusCount()
        {
            lock (padlock)
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("SELECT " + StatusCount.ColumnName_Id + ", "
                        + StatusCount.ColumnName_PendingCount + ", "
                        + StatusCount.ColumnName_RunningCount + ", "
                        + StatusCount.ColumnName_DoneCount + ", "
                        + StatusCount.ColumnName_FailedCount + ", "
                        + StatusCount.ColumnName_WarningCount + ", "
                        + StatusCount.ColumnName_DisabledCount + ", "
                        + StatusCount.ColumnName_StoppedCount + ", "
                        + StatusCount.ColumnName_RejectedCount
                        + " FROM " + Core.Db.StatusCount.DocumentName
                        , conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var statusCount = new StatusCount
                                {
                                    Id = Convert.ToInt64((decimal)reader[StatusCount.ColumnName_Id]),
                                    PendingCount = Convert.ToInt32((decimal)reader[StatusCount.ColumnName_PendingCount]),
                                    RunningCount = Convert.ToInt32(reader[StatusCount.ColumnName_RunningCount]),
                                    DoneCount = Convert.ToInt32(reader[StatusCount.ColumnName_DoneCount]),
                                    FailedCount = Convert.ToInt32(reader[StatusCount.ColumnName_FailedCount]),
                                    WarningCount = Convert.ToInt32(reader[StatusCount.ColumnName_WarningCount]),
                                    DisabledCount = Convert.ToInt32(reader[StatusCount.ColumnName_DisabledCount]),
                                    StoppedCount = Convert.ToInt32(reader[StatusCount.ColumnName_StoppedCount]),
                                    RejectedCount = Convert.ToInt32(reader[StatusCount.ColumnName_RejectedCount])
                                };

                                return statusCount;
                            }
                        }
                    }
                }

                return null;
            }
        }

        public override Core.Db.User GetUser(string username)
        {
            lock (padlock)
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("SELECT " + User.ColumnName_Id + ", "
                        + User.ColumnName_Username + ", "
                        + User.ColumnName_Password + ", "
                        + User.ColumnName_Email + ", "
                        + User.ColumnName_UserProfile + ", "
                        + User.ColumnName_CreatedOn + ", "
                        + User.ColumnName_ModifiedOn
                        + " FROM " + Core.Db.User.DocumentName
                        + " WHERE " + User.ColumnName_Username + " = '" + (username ?? "").Replace("'", "''") + "'"
                        , conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var user = new User
                                {
                                    Id = Convert.ToInt64((decimal)reader[User.ColumnName_Id]),
                                    Username = (string)reader[User.ColumnName_Username],
                                    Password = (string)reader[User.ColumnName_Password],
                                    Email = reader[User.ColumnName_Email] == DBNull.Value ? string.Empty : (string)reader[User.ColumnName_Email],
                                    UserProfile = (UserProfile)(Convert.ToInt32((decimal)reader[User.ColumnName_UserProfile])),
                                    CreatedOn = (DateTime)reader[User.ColumnName_CreatedOn],
                                    ModifiedOn = reader[User.ColumnName_ModifiedOn] == DBNull.Value ? DateTime.MinValue : (DateTime)reader[User.ColumnName_ModifiedOn]
                                };

                                return user;
                            }
                        }
                    }
                }

                return null;
            }
        }

        public override Core.Db.User GetUserById(string userId)
        {
            lock (padlock)
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("SELECT " + User.ColumnName_Id + ", "
                        + User.ColumnName_Username + ", "
                        + User.ColumnName_Password + ", "
                        + User.ColumnName_Email + ", "
                        + User.ColumnName_UserProfile + ", "
                        + User.ColumnName_CreatedOn + ", "
                        + User.ColumnName_ModifiedOn
                        + " FROM " + Core.Db.User.DocumentName
                        + " WHERE " + User.ColumnName_Id + " = '" + int.Parse(userId) + "'"
                        , conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {

                            if (reader.Read())
                            {
                                var user = new User
                                {
                                    Id = Convert.ToInt64((decimal)reader[User.ColumnName_Id]),
                                    Username = (string)reader[User.ColumnName_Username],
                                    Password = (string)reader[User.ColumnName_Password],
                                    Email = reader[User.ColumnName_Email] == DBNull.Value ? string.Empty : (string)reader[User.ColumnName_Email],
                                    UserProfile = (UserProfile)(Convert.ToInt32((decimal)reader[User.ColumnName_UserProfile])),
                                    CreatedOn = (DateTime)reader[User.ColumnName_CreatedOn],
                                    ModifiedOn = reader[User.ColumnName_ModifiedOn] == DBNull.Value ? DateTime.MinValue : (DateTime)reader[User.ColumnName_ModifiedOn]
                                };

                                return user;
                            }
                        }
                    }
                }

                return null;
            }
        }

        public override IEnumerable<Core.Db.User> GetUsers()
        {
            lock (padlock)
            {
                List<User> users = new List<User>();

                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("SELECT " + User.ColumnName_Id + ", "
                        + User.ColumnName_Username + ", "
                        + User.ColumnName_Password + ", "
                        + User.ColumnName_Email + ", "
                        + User.ColumnName_UserProfile + ", "
                        + User.ColumnName_CreatedOn + ", "
                        + User.ColumnName_ModifiedOn
                        + " FROM " + Core.Db.User.DocumentName
                        , conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {

                            while (reader.Read())
                            {
                                var user = new User
                                {
                                    Id = Convert.ToInt64((decimal)reader[User.ColumnName_Id]),
                                    Username = (string)reader[User.ColumnName_Username],
                                    Password = (string)reader[User.ColumnName_Password],
                                    Email = reader[User.ColumnName_Email] == DBNull.Value ? string.Empty : (string)reader[User.ColumnName_Email],
                                    UserProfile = (UserProfile)(Convert.ToInt32((decimal)reader[User.ColumnName_UserProfile])),
                                    CreatedOn = (DateTime)reader[User.ColumnName_CreatedOn],
                                    ModifiedOn = reader[User.ColumnName_ModifiedOn] == DBNull.Value ? DateTime.MinValue : (DateTime)reader[User.ColumnName_ModifiedOn]
                                };

                                users.Add(user);
                            }
                        }
                    }
                }

                return users;
            }
        }

        public override IEnumerable<Core.Db.User> GetUsers(string keyword, UserOrderBy uo)
        {
            lock (padlock)
            {
                List<User> users = new List<User>();

                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("SELECT " + User.ColumnName_Id + ", "
                        + User.ColumnName_Username + ", "
                        + User.ColumnName_Password + ", "
                        + User.ColumnName_Email + ", "
                        + User.ColumnName_UserProfile + ", "
                        + User.ColumnName_CreatedOn + ", "
                        + User.ColumnName_ModifiedOn
                        + " FROM " + Core.Db.User.DocumentName
                        + " WHERE " + "LOWER(" + User.ColumnName_Username + ")" + " LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%'"
                        + " ORDER BY " + User.ColumnName_Username + (uo == UserOrderBy.UsernameAscending ? " ASC" : " DESC")
                        , conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {

                            while (reader.Read())
                            {
                                var user = new User
                                {
                                    Id = Convert.ToInt64((decimal)reader[User.ColumnName_Id]),
                                    Username = (string)reader[User.ColumnName_Username],
                                    Password = (string)reader[User.ColumnName_Password],
                                    Email = reader[User.ColumnName_Email] == DBNull.Value ? string.Empty : (string)reader[User.ColumnName_Email],
                                    UserProfile = (UserProfile)(Convert.ToInt32((decimal)reader[User.ColumnName_UserProfile])),
                                    CreatedOn = (DateTime)reader[User.ColumnName_CreatedOn],
                                    ModifiedOn = reader[User.ColumnName_ModifiedOn] == DBNull.Value ? DateTime.MinValue : (DateTime)reader[User.ColumnName_ModifiedOn]
                                };

                                users.Add(user);
                            }
                        }
                    }
                }

                return users;
            }
        }

        public override IEnumerable<string> GetUserWorkflows(string userId)
        {
            lock (padlock)
            {
                List<string> workflowIds = new List<string>();

                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("SELECT " + UserWorkflow.ColumnName_Id + ", "
                        + UserWorkflow.ColumnName_UserId + ", "
                        + UserWorkflow.ColumnName_WorkflowId
                        + " FROM " + Core.Db.UserWorkflow.DocumentName
                        + " WHERE " + UserWorkflow.ColumnName_UserId + " = " + int.Parse(userId)
                        , conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var workflowId = Convert.ToInt64((decimal)reader[UserWorkflow.ColumnName_WorkflowId]);

                                workflowIds.Add(workflowId.ToString());
                            }
                        }
                    }
                }

                return workflowIds;
            }
        }

        public override Core.Db.Workflow GetWorkflow(string id)
        {
            lock (padlock)
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("SELECT " + Workflow.ColumnName_Id + ", "
                        + Workflow.ColumnName_Xml
                        + " FROM " + Core.Db.Workflow.DocumentName
                        + " WHERE " + Workflow.ColumnName_Id + " = " + int.Parse(id), conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {

                            if (reader.Read())
                            {
                                var workflow = new Workflow
                                {
                                    Id = Convert.ToInt64((decimal)reader[Workflow.ColumnName_Id]),
                                    Xml = (string)reader[Workflow.ColumnName_Xml]
                                };

                                return workflow;
                            }
                        }
                    }
                }

                return null;
            }
        }

        public override IEnumerable<Core.Db.Workflow> GetWorkflows()
        {
            lock (padlock)
            {
                List<Core.Db.Workflow> workflows = new List<Core.Db.Workflow>();

                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("SELECT " + Workflow.ColumnName_Id + ", "
                        + Workflow.ColumnName_Xml
                        + " FROM " + Core.Db.Workflow.DocumentName, conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {

                            while (reader.Read())
                            {
                                var workflow = new Workflow
                                {
                                    Id = Convert.ToInt64((decimal)reader[Workflow.ColumnName_Id]),
                                    Xml = (string)reader[Workflow.ColumnName_Xml]
                                };

                                workflows.Add(workflow);
                            }
                        }
                    }
                }

                return workflows;
            }
        }

        private void IncrementStatusCountColumn(string statusCountColumnName)
        {
            lock (padlock)
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("UPDATE " + Core.Db.StatusCount.DocumentName + " SET " + statusCountColumnName + " = " + statusCountColumnName + " + 1", conn))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public override void IncrementDisabledCount()
        {
            IncrementStatusCountColumn(StatusCount.ColumnName_DisabledCount);
        }

        public override void IncrementRejectedCount()
        {
            IncrementStatusCountColumn(StatusCount.ColumnName_RejectedCount);
        }

        public override void IncrementDoneCount()
        {
            IncrementStatusCountColumn(StatusCount.ColumnName_DoneCount);
        }

        public override void IncrementFailedCount()
        {
            IncrementStatusCountColumn(StatusCount.ColumnName_FailedCount);
        }

        public override void IncrementPendingCount()
        {
            IncrementStatusCountColumn(StatusCount.ColumnName_PendingCount);
        }

        public override void IncrementRunningCount()
        {
            IncrementStatusCountColumn(StatusCount.ColumnName_RunningCount);
        }

        public override void IncrementStoppedCount()
        {
            IncrementStatusCountColumn(StatusCount.ColumnName_StoppedCount);
        }

        public override void IncrementWarningCount()
        {
            IncrementStatusCountColumn(StatusCount.ColumnName_WarningCount);
        }

        private void DecrementStatusCountColumn(string statusCountColumnName)
        {
            lock (padlock)
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("UPDATE " + Core.Db.StatusCount.DocumentName + " SET " + statusCountColumnName + " = " + statusCountColumnName + " - 1", conn))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public override void DecrementPendingCount()
        {
            DecrementStatusCountColumn(StatusCount.ColumnName_PendingCount);
        }

        public override void DecrementRunningCount()
        {
            DecrementStatusCountColumn(StatusCount.ColumnName_RunningCount);
        }

        public override void InsertWorkflowInstance(Core.Db.Entry entry)
        {
            lock (padlock)
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("INSERT INTO " + Core.Db.Entry.DocumentName + "("
                        + Entry.ColumnName_Name + ", "
                        + Entry.ColumnName_Description + ", "
                        + Entry.ColumnName_LaunchType + ", "
                        + Entry.ColumnName_StatusDate + ", "
                        + Entry.ColumnName_Status + ", "
                        + Entry.ColumnName_WorkflowId + ", "
                        + Entry.ColumnName_JobId + ", "
                        + Entry.ColumnName_Logs + ") VALUES("
                        + "'" + (entry.Name ?? "").Replace("'", "''") + "'" + ", "
                        + "'" + (entry.Description ?? "").Replace("'", "''") + "'" + ", "
                        + (int)entry.LaunchType + ", "
                        + "TO_TIMESTAMP('" + entry.StatusDate.ToString(dateTimeFormat) + "'" + ", 'YYYY-MM-DD HH24:MI:SS.FF'), "
                        + (int)entry.Status + ", "
                        + entry.WorkflowId + ", "
                        + "'" + (entry.JobId ?? "") + "', "
                        + "'" + (entry.Logs ?? "").Replace("'", "''") + "'" + ")"
                        , conn))
                    {

                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public override void InsertHistoryEntry(Core.Db.HistoryEntry entry)
        {
            lock (padlock)
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("INSERT INTO " + Core.Db.HistoryEntry.DocumentName + "("
                        + HistoryEntry.ColumnName_Name + ", "
                        + HistoryEntry.ColumnName_Description + ", "
                        + HistoryEntry.ColumnName_LaunchType + ", "
                        + HistoryEntry.ColumnName_StatusDate + ", "
                        + HistoryEntry.ColumnName_Status + ", "
                        + HistoryEntry.ColumnName_WorkflowId + ", "
                        + HistoryEntry.ColumnName_Logs + ") VALUES("
                        + "'" + (entry.Name ?? "").Replace("'", "''") + "'" + ", "
                        + "'" + (entry.Description ?? "").Replace("'", "''") + "'" + ", "
                        + (int)entry.LaunchType + ", "
                        + "TO_TIMESTAMP('" + entry.StatusDate.ToString(dateTimeFormat) + "'" + ", 'YYYY-MM-DD HH24:MI:SS.FF'), "
                        + (int)entry.Status + ", "
                        + entry.WorkflowId + ", "
                        + "'" + (entry.Logs ?? "").Replace("'", "''") + "'" + ")"
                        , conn))
                    {

                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public override void InsertUser(Core.Db.User user)
        {
            lock (padlock)
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("INSERT INTO " + Core.Db.User.DocumentName + "("
                        + User.ColumnName_Username + ", "
                        + User.ColumnName_Password + ", "
                        + User.ColumnName_UserProfile + ", "
                        + User.ColumnName_Email + ", "
                        + User.ColumnName_CreatedOn + ", "
                        + User.ColumnName_ModifiedOn + ") VALUES("
                        + "'" + (user.Username ?? "").Replace("'", "''") + "'" + ", "
                        + "'" + (user.Password ?? "").Replace("'", "''") + "'" + ", "
                        + (int)user.UserProfile + ", "
                        + "'" + (user.Email ?? "").Replace("'", "''") + "'" + ", "
                        + "TO_TIMESTAMP(" + "'" + DateTime.Now.ToString(dateTimeFormat) + "'" + ", 'YYYY-MM-DD HH24:MI:SS.FF')" + ", "
                        + (user.ModifiedOn == DateTime.MinValue ? "NULL" : ("TO_TIMESTAMP(" + "'" + user.ModifiedOn.ToString(dateTimeFormat) + "'" + ", 'YYYY-MM-DD HH24:MI:SS.FF')")) + ")"
                        , conn))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public override void InsertUserWorkflowRelation(Core.Db.UserWorkflow userWorkflow)
        {
            lock (padlock)
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("INSERT INTO " + Core.Db.UserWorkflow.DocumentName + "("
                        + UserWorkflow.ColumnName_UserId + ", "
                        + UserWorkflow.ColumnName_WorkflowId + ") VALUES("
                        + int.Parse(userWorkflow.UserId) + ", "
                        + int.Parse(userWorkflow.WorkflowId) + ")"
                        , conn))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        private IEnumerable<string> ToChuncks(string str, int maxChunkSize)
        {
            lock (padlock)
            {
                for (int i = 0; i < str.Length; i += maxChunkSize)
                {
                    yield return str.Substring(i, Math.Min(maxChunkSize, str.Length - i));
                }
            }
        }

        private string ToCLOB(Core.Db.Workflow workflow)
        {
            lock (padlock)
            {
                var xml = (workflow.Xml ?? "").Replace("'", "''");
                var chunkSize = 4000;
                var builder = new StringBuilder();
                var chunks = ToChuncks(xml, chunkSize).ToArray();

                for (var i = 0; i < chunks.Length; i++)
                {
                    var chunk = chunks[i];
                    builder.Append("TO_CLOB(").Append("'").Append(chunk).Append("')");
                    if (i < chunks.Length - 1)
                    {
                        builder.Append(" || ");
                    }
                }

                var xmlVal = builder.ToString();

                return xmlVal;
            }
        }

        public override string InsertWorkflow(Core.Db.Workflow workflow)
        {
            lock (padlock)
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();
                    var xml = ToCLOB(workflow);

                    using (var command = new OracleCommand("INSERT INTO " + Core.Db.Workflow.DocumentName + "("
                        + Workflow.ColumnName_Xml + ") VALUES("
                        + xml + ") RETURNING " + Workflow.ColumnName_Id + " INTO :id"
                        , conn))
                    {
                        command.Parameters.Add(new OracleParameter
                        {
                            ParameterName = ":id",
                            DbType = System.Data.DbType.Decimal,
                            Direction = System.Data.ParameterDirection.Output
                        });

                        command.ExecuteNonQuery();

                        var id = Convert.ToInt64(command.Parameters[":id"].Value).ToString();

                        return id.ToString();
                    }
                }
            }
        }

        public override void UpdateEntry(string id, Core.Db.Entry entry)
        {
            lock (padlock)
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("UPDATE " + Core.Db.Entry.DocumentName + " SET "
                        + Entry.ColumnName_Name + " = '" + (entry.Name ?? "").Replace("'", "''") + "', "
                        + Entry.ColumnName_Description + " = '" + (entry.Description ?? "").Replace("'", "''") + "', "
                        + Entry.ColumnName_LaunchType + " = " + (int)entry.LaunchType + ", "
                        + Entry.ColumnName_StatusDate + " = TO_TIMESTAMP('" + entry.StatusDate.ToString(dateTimeFormat) + "', 'YYYY-MM-DD HH24:MI:SS.FF'), "
                        + Entry.ColumnName_Status + " = " + (int)entry.Status + ", "
                        + Entry.ColumnName_WorkflowId + " = " + entry.WorkflowId + ", "
                        + Entry.ColumnName_JobId + " = '" + (entry.JobId ?? "") + "', "
                        + Entry.ColumnName_Logs + " = '" + (entry.Logs ?? "").Replace("'", "''") + "'"
                        + " WHERE "
                        + Entry.ColumnName_Id + " = " + int.Parse(id)
                        , conn))
                    {

                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public override void UpdatePassword(string username, string password)
        {
            lock (padlock)
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("UPDATE " + Core.Db.User.DocumentName + " SET "
                        + User.ColumnName_Password + " = '" + (password ?? "").Replace("'", "''") + "'"
                        + " WHERE "
                        + User.ColumnName_Username + " = '" + (username ?? "").Replace("'", "''") + "'"
                        , conn))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public override void UpdateUser(string id, Core.Db.User user)
        {
            lock (padlock)
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("UPDATE " + Core.Db.User.DocumentName + " SET "
                        + User.ColumnName_Username + " = '" + (user.Username ?? "").Replace("'", "''") + "', "
                        + User.ColumnName_Password + " = '" + (user.Password ?? "").Replace("'", "''") + "', "
                        + User.ColumnName_UserProfile + " = " + (int)user.UserProfile + ", "
                        + User.ColumnName_Email + " = '" + (user.Email ?? "").Replace("'", "''") + "', "
                        + User.ColumnName_CreatedOn + " = TO_TIMESTAMP('" + user.CreatedOn.ToString(dateTimeFormat) + "', 'YYYY-MM-DD HH24:MI:SS.FF')" + ", "
                        + User.ColumnName_ModifiedOn + " = TO_TIMESTAMP('" + DateTime.Now.ToString(dateTimeFormat) + "', 'YYYY-MM-DD HH24:MI:SS.FF')"
                        + " WHERE "
                        + User.ColumnName_Id + " = " + int.Parse(id)
                        , conn))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public override void UpdateUsernameAndEmailAndUserProfile(string userId, string username, string email, UserProfile up)
        {
            using (var conn = new OracleConnection(connectionString))
            {
                conn.Open();

                using (var command = new OracleCommand("UPDATE " + Core.Db.User.DocumentName + " SET "
                    + User.ColumnName_Username + " = '" + (username ?? "").Replace("'", "''") + "', "
                    + User.ColumnName_UserProfile + " = " + (int)up + ", "
                    + User.ColumnName_Email + " = '" + (email ?? "").Replace("'", "''") + "', "
                    + User.ColumnName_ModifiedOn + " = TO_TIMESTAMP('" + DateTime.Now.ToString(dateTimeFormat) + "', 'YYYY-MM-DD HH24:MI:SS.FF')"
                    + " WHERE "
                    + User.ColumnName_Id + " = " + int.Parse(userId)
                    , conn))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public override void UpdateWorkflow(string dbId, Core.Db.Workflow workflow)
        {
            lock (padlock)
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    var xml = ToCLOB(workflow);

                    using (var command = new OracleCommand("UPDATE " + Core.Db.Workflow.DocumentName + " SET "
                        + Workflow.ColumnName_Xml + " = " + xml
                        + " WHERE "
                        + User.ColumnName_Id + " = " + int.Parse(dbId)
                        , conn))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public override string GetEntryLogs(string entryId)
        {
            lock (padlock)
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("SELECT " + Entry.ColumnName_Logs
                        + " FROM " + Core.Db.Entry.DocumentName
                        + " WHERE "
                        + Entry.ColumnName_Id + " = " + int.Parse(entryId)
                        , conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var logs = reader[Entry.ColumnName_Logs] == DBNull.Value ? string.Empty : (string)reader[Entry.ColumnName_Logs];
                                return logs;
                            }
                        }
                    }

                }

                return null;
            }
        }

        public override string GetHistoryEntryLogs(string entryId)
        {
            lock (padlock)
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("SELECT " + HistoryEntry.ColumnName_Logs
                        + " FROM " + Core.Db.HistoryEntry.DocumentName
                        + " WHERE "
                        + HistoryEntry.ColumnName_Id + " = " + int.Parse(entryId)
                        , conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var logs = reader[Entry.ColumnName_Logs] == DBNull.Value ? string.Empty : (string)reader[HistoryEntry.ColumnName_Logs];
                                return logs;
                            }
                        }
                    }

                }

                return null;
            }
        }

        public override IEnumerable<Core.Db.User> GetNonRestricedUsers()
        {
            lock (padlock)
            {
                List<User> users = new List<User>();

                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("SELECT "
                        + User.ColumnName_Id + ", "
                        + User.ColumnName_Username + ", "
                        + User.ColumnName_Password + ", "
                        + User.ColumnName_Email + ", "
                        + User.ColumnName_UserProfile + ", "
                        + User.ColumnName_CreatedOn + ", "
                        + User.ColumnName_ModifiedOn
                        + " FROM " + Core.Db.User.DocumentName
                        + " WHERE (" + User.ColumnName_UserProfile + " = " + (int)UserProfile.SuperAdministrator
                        + " OR " + User.ColumnName_UserProfile + " = " + (int)UserProfile.Administrator + ")"
                        + " ORDER BY " + User.ColumnName_Username
                        , conn))
                    {

                        using (var reader = command.ExecuteReader())
                        {

                            while (reader.Read())
                            {
                                var admin = new User
                                {
                                    Id = Convert.ToInt64((decimal)reader[User.ColumnName_Id]),
                                    Username = (string)reader[User.ColumnName_Username],
                                    Password = (string)reader[User.ColumnName_Password],
                                    Email = reader[User.ColumnName_Email] == DBNull.Value ? string.Empty : (string)reader[User.ColumnName_Email],
                                    UserProfile = (UserProfile)(Convert.ToInt32((decimal)reader[User.ColumnName_UserProfile])),
                                    CreatedOn = (DateTime)reader[User.ColumnName_CreatedOn],
                                    ModifiedOn = reader[User.ColumnName_ModifiedOn] == DBNull.Value ? DateTime.MinValue : (DateTime)reader[User.ColumnName_ModifiedOn]
                                };

                                users.Add(admin);
                            }
                        }
                    }
                }

                return users;
            }
        }

        public override string InsertRecord(Core.Db.Record record)
        {
            lock (padlock)
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("INSERT INTO " + Core.Db.Record.DocumentName + "("
                        + Record.ColumnName_Name + ", "
                        + Record.ColumnName_Description + ", "
                        + Record.ColumnName_Approved + ", "
                        + Record.ColumnName_StartDate + ", "
                        + Record.ColumnName_EndDate + ", "
                        + Record.ColumnName_Comments + ", "
                        + Record.ColumnName_ManagerComments + ", "
                        + Record.ColumnName_CreatedBy + ", "
                        + Record.ColumnName_CreatedOn + ", "
                        + Record.ColumnName_ModifiedBy + ", "
                        + Record.ColumnName_ModifiedOn + ", "
                        + Record.ColumnName_AssignedTo + ", "
                        + Record.ColumnName_AssignedOn + ")"
                        + " VALUES("
                        + "'" + (record.Name ?? "").Replace("'", "''") + "'" + ", "
                        + "'" + (record.Description ?? "").Replace("'", "''") + "'" + ", "
                        + (record.Approved ? "1" : "0") + ", "
                        + (record.StartDate == null ? "NULL" : "TO_TIMESTAMP(" + "'" + record.StartDate.Value.ToString(dateTimeFormat) + "'" + ", 'YYYY-MM-DD HH24:MI:SS.FF')") + ", "
                        + (record.EndDate == null ? "NULL" : "TO_TIMESTAMP(" + "'" + record.EndDate.Value.ToString(dateTimeFormat) + "'" + ", 'YYYY-MM-DD HH24:MI:SS.FF')") + ", "
                        + "'" + (record.Comments ?? "").Replace("'", "''") + "'" + ", "
                        + "'" + (record.ManagerComments ?? "").Replace("'", "''") + "'" + ", "
                        + int.Parse(record.CreatedBy) + ", "
                        + "TO_TIMESTAMP(" + "'" + DateTime.Now.ToString(dateTimeFormat) + "'" + ", 'YYYY-MM-DD HH24:MI:SS.FF')" + ", "
                        + (string.IsNullOrEmpty(record.ModifiedBy) ? "NULL" : int.Parse(record.ModifiedBy).ToString()) + ", "
                        + (record.ModifiedOn == null ? "NULL" : "TO_TIMESTAMP(" + "'" + record.ModifiedOn.Value.ToString(dateTimeFormat) + "'" + ", 'YYYY-MM-DD HH24:MI:SS.FF')") + ", "
                         + (string.IsNullOrEmpty(record.AssignedTo) ? "NULL" : int.Parse(record.AssignedTo).ToString()) + ", "
                        + (record.AssignedOn == null ? "NULL" : "TO_TIMESTAMP(" + "'" + record.AssignedOn.Value.ToString(dateTimeFormat) + "'" + ", 'YYYY-MM-DD HH24:MI:SS.FF')") + ")"
                        + " RETURNING " + Record.ColumnName_Id + " INTO :id"
                        , conn))
                    {
                        command.Parameters.Add(new OracleParameter
                        {
                            ParameterName = ":id",
                            DbType = System.Data.DbType.Decimal,
                            Direction = System.Data.ParameterDirection.Output
                        });

                        command.ExecuteNonQuery();

                        var id = Convert.ToInt64(command.Parameters[":id"].Value).ToString();

                        return id.ToString();
                    }
                }
            }
        }

        public override void UpdateRecord(string recordId, Core.Db.Record record)
        {
            lock (padlock)
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("UPDATE " + Core.Db.Record.DocumentName + " SET "
                        + Record.ColumnName_Name + " = '" + (record.Name ?? "").Replace("'", "''") + "', "
                        + Record.ColumnName_Description + " = '" + (record.Description ?? "").Replace("'", "''") + "', "
                        + Record.ColumnName_Approved + " = " + (record.Approved ? "1" : "0") + ", "
                        + Record.ColumnName_StartDate + " = " + (record.StartDate == null ? "NULL" : "TO_TIMESTAMP(" + "'" + record.StartDate.Value.ToString(dateTimeFormat) + "'" + ", 'YYYY-MM-DD HH24:MI:SS.FF')") + ", "
                        + Record.ColumnName_EndDate + " = " + (record.EndDate == null ? "NULL" : "TO_TIMESTAMP(" + "'" + record.EndDate.Value.ToString(dateTimeFormat) + "'" + ", 'YYYY-MM-DD HH24:MI:SS.FF')") + ", "
                        + Record.ColumnName_Comments + " = '" + (record.Comments ?? "").Replace("'", "''") + "', "
                        + Record.ColumnName_ManagerComments + " = '" + (record.ManagerComments ?? "").Replace("'", "''") + "', "
                        + Record.ColumnName_CreatedBy + " = " + int.Parse(record.CreatedBy) + ", "
                        + Record.ColumnName_ModifiedBy + " = " + (string.IsNullOrEmpty(record.ModifiedBy) ? "NULL" : int.Parse(record.ModifiedBy).ToString()) + ", "
                        + Record.ColumnName_ModifiedOn + " = " + "TO_TIMESTAMP(" + "'" + DateTime.Now.ToString(dateTimeFormat) + "'" + ", 'YYYY-MM-DD HH24:MI:SS.FF')" + ", "
                        + Record.ColumnName_AssignedTo + " = " + (string.IsNullOrEmpty(record.AssignedTo) ? "NULL" : int.Parse(record.AssignedTo).ToString()) + ", "
                        + Record.ColumnName_AssignedOn + " = " + (record.AssignedOn == null ? "NULL" : "TO_TIMESTAMP(" + "'" + record.AssignedOn.Value.ToString(dateTimeFormat) + "'" + ", 'YYYY-MM-DD HH24:MI:SS.FF')")
                        + " WHERE "
                        + Record.ColumnName_Id + " = " + int.Parse(recordId)
                        , conn))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public override void DeleteRecords(string[] recordIds)
        {
            lock (padlock)
            {
                if (recordIds.Length > 0)
                {
                    using (var conn = new OracleConnection(connectionString))
                    {
                        conn.Open();

                        var builder = new StringBuilder("(");

                        for (int i = 0; i < recordIds.Length; i++)
                        {
                            var id = recordIds[i];
                            builder.Append(id);
                            if (i < recordIds.Length - 1)
                            {
                                builder.Append(", ");
                            }
                            else
                            {
                                builder.Append(")");
                            }
                        }

                        using (var command = new OracleCommand("DELETE FROM " + Core.Db.Record.DocumentName
                            + " WHERE " + Record.ColumnName_Id + " IN " + builder.ToString(), conn))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        public override Core.Db.Record GetRecord(string id)
        {
            lock (padlock)
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("SELECT "
                        + Record.ColumnName_Id + ", "
                        + Record.ColumnName_Name + ", "
                        + Record.ColumnName_Description + ", "
                        + Record.ColumnName_Approved + ", "
                        + Record.ColumnName_StartDate + ", "
                        + Record.ColumnName_EndDate + ", "
                        + Record.ColumnName_Comments + ", "
                        + Record.ColumnName_ManagerComments + ", "
                        + Record.ColumnName_CreatedBy + ", "
                        + Record.ColumnName_CreatedOn + ", "
                        + Record.ColumnName_ModifiedBy + ", "
                        + Record.ColumnName_ModifiedOn + ", "
                        + Record.ColumnName_AssignedTo + ", "
                        + Record.ColumnName_AssignedOn
                        + " FROM " + Core.Db.Record.DocumentName
                        + " WHERE " + Record.ColumnName_Id + " = " + int.Parse(id)
                        , conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var record = new Record
                                {
                                    Id = Convert.ToInt64((decimal)reader[Record.ColumnName_Id]),
                                    Name = reader[Record.ColumnName_Name] == DBNull.Value ? null : (string)reader[Record.ColumnName_Name],
                                    Description = reader[Record.ColumnName_Description] == DBNull.Value ? null : (string)reader[Record.ColumnName_Description],
                                    Approved = ((short)reader[Record.ColumnName_Approved]) == 1 ? true : false,
                                    StartDate = reader[Record.ColumnName_StartDate] == DBNull.Value ? null : (DateTime?)reader[Record.ColumnName_StartDate],
                                    EndDate = reader[Record.ColumnName_EndDate] == DBNull.Value ? null : (DateTime?)reader[Record.ColumnName_EndDate],
                                    Comments = reader[Record.ColumnName_Comments] == DBNull.Value ? null : (string)reader[Record.ColumnName_Comments],
                                    ManagerComments = reader[Record.ColumnName_ManagerComments] == DBNull.Value ? null : (string)reader[Record.ColumnName_ManagerComments],
                                    CreatedBy = Convert.ToInt64((decimal)reader[Record.ColumnName_CreatedBy]).ToString(),
                                    CreatedOn = (DateTime)reader[Record.ColumnName_CreatedOn],
                                    ModifiedBy = reader[Record.ColumnName_ModifiedBy] == DBNull.Value ? string.Empty : Convert.ToInt64((decimal)reader[Record.ColumnName_ModifiedBy]).ToString(),
                                    ModifiedOn = reader[Record.ColumnName_ModifiedOn] == DBNull.Value ? null : (DateTime?)reader[Record.ColumnName_ModifiedOn],
                                    AssignedTo = reader[Record.ColumnName_AssignedTo] == DBNull.Value ? string.Empty : Convert.ToInt64((decimal)reader[Record.ColumnName_AssignedTo]).ToString(),
                                    AssignedOn = reader[Record.ColumnName_AssignedOn] == DBNull.Value ? null : (DateTime?)reader[Record.ColumnName_AssignedOn]
                                };

                                return record;
                            }
                        }
                    }
                }

                return null;
            }
        }

        public override IEnumerable<Core.Db.Record> GetRecords(string keyword)
        {
            lock (padlock)
            {
                List<Record> records = new List<Record>();

                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("SELECT "
                        + Record.ColumnName_Id + ", "
                        + Record.ColumnName_Name + ", "
                        + Record.ColumnName_Description + ", "
                        + Record.ColumnName_Approved + ", "
                        + Record.ColumnName_StartDate + ", "
                        + Record.ColumnName_EndDate + ", "
                        + Record.ColumnName_Comments + ", "
                        + Record.ColumnName_ManagerComments + ", "
                        + Record.ColumnName_CreatedBy + ", "
                        + Record.ColumnName_CreatedOn + ", "
                        + Record.ColumnName_ModifiedBy + ", "
                        + Record.ColumnName_ModifiedOn + ", "
                        + Record.ColumnName_AssignedTo + ", "
                        + Record.ColumnName_AssignedOn
                        + " FROM " + Core.Db.Record.DocumentName
                        + " WHERE " + "LOWER(" + Record.ColumnName_Name + ")" + " LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%'"
                        + " OR " + "LOWER(" + Record.ColumnName_Description + ")" + " LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%'"
                        + " ORDER BY " + Record.ColumnName_CreatedOn + " DESC"
                        , conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var record = new Record
                                {
                                    Id = Convert.ToInt64((decimal)reader[Record.ColumnName_Id]),
                                    Name = reader[Record.ColumnName_Name] == DBNull.Value ? null : (string)reader[Record.ColumnName_Name],
                                    Description = reader[Record.ColumnName_Description] == DBNull.Value ? null : (string)reader[Record.ColumnName_Description],
                                    Approved = ((short)reader[Record.ColumnName_Approved]) == 1 ? true : false,
                                    StartDate = reader[Record.ColumnName_StartDate] == DBNull.Value ? null : (DateTime?)reader[Record.ColumnName_StartDate],
                                    EndDate = reader[Record.ColumnName_EndDate] == DBNull.Value ? null : (DateTime?)reader[Record.ColumnName_EndDate],
                                    Comments = reader[Record.ColumnName_Comments] == DBNull.Value ? null : (string)reader[Record.ColumnName_Comments],
                                    ManagerComments = reader[Record.ColumnName_ManagerComments] == DBNull.Value ? null : (string)reader[Record.ColumnName_ManagerComments],
                                    CreatedBy = Convert.ToInt64((decimal)reader[Record.ColumnName_CreatedBy]).ToString(),
                                    CreatedOn = (DateTime)reader[Record.ColumnName_CreatedOn],
                                    ModifiedBy = reader[Record.ColumnName_ModifiedBy] == DBNull.Value ? string.Empty : Convert.ToInt64((decimal)reader[Record.ColumnName_ModifiedBy]).ToString(),
                                    ModifiedOn = reader[Record.ColumnName_ModifiedOn] == DBNull.Value ? null : (DateTime?)reader[Record.ColumnName_ModifiedOn],
                                    AssignedTo = reader[Record.ColumnName_AssignedTo] == DBNull.Value ? string.Empty : Convert.ToInt64((decimal)reader[Record.ColumnName_AssignedTo]).ToString(),
                                    AssignedOn = reader[Record.ColumnName_AssignedOn] == DBNull.Value ? null : (DateTime?)reader[Record.ColumnName_AssignedOn]
                                };

                                records.Add(record);
                            }
                        }
                    }
                }

                return records;
            }
        }

        public override IEnumerable<Core.Db.Record> GetRecordsCreatedBy(string createdBy)
        {
            lock (padlock)
            {
                List<Record> records = new List<Record>();

                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("SELECT "
                        + Record.ColumnName_Id + ", "
                        + Record.ColumnName_Name + ", "
                        + Record.ColumnName_Description + ", "
                        + Record.ColumnName_Approved + ", "
                        + Record.ColumnName_StartDate + ", "
                        + Record.ColumnName_EndDate + ", "
                        + Record.ColumnName_Comments + ", "
                        + Record.ColumnName_ManagerComments + ", "
                        + Record.ColumnName_CreatedBy + ", "
                        + Record.ColumnName_CreatedOn + ", "
                        + Record.ColumnName_ModifiedBy + ", "
                        + Record.ColumnName_ModifiedOn + ", "
                        + Record.ColumnName_AssignedTo + ", "
                        + Record.ColumnName_AssignedOn
                        + " FROM " + Core.Db.Record.DocumentName
                        + " WHERE " + Record.ColumnName_CreatedBy + " = " + int.Parse(createdBy)
                        + " ORDER BY " + Record.ColumnName_Name + " ASC"
                        , conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var record = new Record
                                {
                                    Id = Convert.ToInt64((decimal)reader[Record.ColumnName_Id]),
                                    Name = reader[Record.ColumnName_Name] == DBNull.Value ? null : (string)reader[Record.ColumnName_Name],
                                    Description = reader[Record.ColumnName_Description] == DBNull.Value ? null : (string)reader[Record.ColumnName_Description],
                                    Approved = ((short)reader[Record.ColumnName_Approved]) == 1 ? true : false,
                                    StartDate = reader[Record.ColumnName_StartDate] == DBNull.Value ? null : (DateTime?)reader[Record.ColumnName_StartDate],
                                    EndDate = reader[Record.ColumnName_EndDate] == DBNull.Value ? null : (DateTime?)reader[Record.ColumnName_EndDate],
                                    Comments = reader[Record.ColumnName_Comments] == DBNull.Value ? null : (string)reader[Record.ColumnName_Comments],
                                    ManagerComments = reader[Record.ColumnName_ManagerComments] == DBNull.Value ? null : (string)reader[Record.ColumnName_ManagerComments],
                                    CreatedBy = Convert.ToInt64((decimal)reader[Record.ColumnName_CreatedBy]).ToString(),
                                    CreatedOn = (DateTime)reader[Record.ColumnName_CreatedOn],
                                    ModifiedBy = reader[Record.ColumnName_ModifiedBy] == DBNull.Value ? string.Empty : Convert.ToInt64((decimal)reader[Record.ColumnName_ModifiedBy]).ToString(),
                                    ModifiedOn = reader[Record.ColumnName_ModifiedOn] == DBNull.Value ? null : (DateTime?)reader[Record.ColumnName_ModifiedOn],
                                    AssignedTo = reader[Record.ColumnName_AssignedTo] == DBNull.Value ? string.Empty : Convert.ToInt64((decimal)reader[Record.ColumnName_AssignedTo]).ToString(),
                                    AssignedOn = reader[Record.ColumnName_AssignedOn] == DBNull.Value ? null : (DateTime?)reader[Record.ColumnName_AssignedOn]
                                };

                                records.Add(record);
                            }
                        }
                    }
                }

                return records;
            }
        }

        public override IEnumerable<Core.Db.Record> GetRecordsCreatedByOrAssignedTo(string createdBy, string assingedTo, string keyword)
        {
            lock (padlock)
            {
                List<Record> records = new List<Record>();

                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("SELECT "
                        + Record.ColumnName_Id + ", "
                        + Record.ColumnName_Name + ", "
                        + Record.ColumnName_Description + ", "
                        + Record.ColumnName_Approved + ", "
                        + Record.ColumnName_StartDate + ", "
                        + Record.ColumnName_EndDate + ", "
                        + Record.ColumnName_Comments + ", "
                        + Record.ColumnName_ManagerComments + ", "
                        + Record.ColumnName_CreatedBy + ", "
                        + Record.ColumnName_CreatedOn + ", "
                        + Record.ColumnName_ModifiedBy + ", "
                        + Record.ColumnName_ModifiedOn + ", "
                        + Record.ColumnName_AssignedTo + ", "
                        + Record.ColumnName_AssignedOn
                        + " FROM " + Core.Db.Record.DocumentName
                        + " WHERE " + "(LOWER(" + Record.ColumnName_Name + ")" + " LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%'"
                        + " OR " + "LOWER(" + Record.ColumnName_Description + ")" + " LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%')"
                        + " AND (" + Record.ColumnName_CreatedBy + " = " + int.Parse(createdBy) + " OR " + Record.ColumnName_AssignedTo + " = " + int.Parse(assingedTo) + ")"
                        + " ORDER BY " + Record.ColumnName_CreatedOn + " DESC"
                        , conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var record = new Record
                                {
                                    Id = Convert.ToInt64((decimal)reader[Record.ColumnName_Id]),
                                    Name = reader[Record.ColumnName_Name] == DBNull.Value ? null : (string)reader[Record.ColumnName_Name],
                                    Description = reader[Record.ColumnName_Description] == DBNull.Value ? null : (string)reader[Record.ColumnName_Description],
                                    Approved = ((short)reader[Record.ColumnName_Approved]) == 1 ? true : false,
                                    StartDate = reader[Record.ColumnName_StartDate] == DBNull.Value ? null : (DateTime?)reader[Record.ColumnName_StartDate],
                                    EndDate = reader[Record.ColumnName_EndDate] == DBNull.Value ? null : (DateTime?)reader[Record.ColumnName_EndDate],
                                    Comments = reader[Record.ColumnName_Comments] == DBNull.Value ? null : (string)reader[Record.ColumnName_Comments],
                                    ManagerComments = reader[Record.ColumnName_ManagerComments] == DBNull.Value ? null : (string)reader[Record.ColumnName_ManagerComments],
                                    CreatedBy = Convert.ToInt64((decimal)reader[Record.ColumnName_CreatedBy]).ToString(),
                                    CreatedOn = (DateTime)reader[Record.ColumnName_CreatedOn],
                                    ModifiedBy = reader[Record.ColumnName_ModifiedBy] == DBNull.Value ? string.Empty : Convert.ToInt64((decimal)reader[Record.ColumnName_ModifiedBy]).ToString(),
                                    ModifiedOn = reader[Record.ColumnName_ModifiedOn] == DBNull.Value ? null : (DateTime?)reader[Record.ColumnName_ModifiedOn],
                                    AssignedTo = reader[Record.ColumnName_AssignedTo] == DBNull.Value ? string.Empty : Convert.ToInt64((decimal)reader[Record.ColumnName_AssignedTo]).ToString(),
                                    AssignedOn = reader[Record.ColumnName_AssignedOn] == DBNull.Value ? null : (DateTime?)reader[Record.ColumnName_AssignedOn]
                                };

                                records.Add(record);
                            }
                        }
                    }
                }

                return records;
            }
        }

        public override string InsertVersion(Core.Db.Version version)
        {
            lock (padlock)
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("INSERT INTO " + Core.Db.Version.DocumentName + "("
                        + Version.ColumnName_RecordId + ", "
                        + Version.ColumnName_FilePath + ", "
                        + Version.ColumnName_CreatedOn + ")"
                        + " VALUES("
                        + int.Parse(version.RecordId) + ", "
                        + "'" + (version.FilePath ?? "").Replace("'", "''") + "'" + ", "
                        + "TO_TIMESTAMP(" + "'" + DateTime.Now.ToString(dateTimeFormat) + "'" + ", 'YYYY-MM-DD HH24:MI:SS.FF')" + ")"
                        + " RETURNING " + Version.ColumnName_Id + " INTO :id"
                        , conn))
                    {
                        command.Parameters.Add(new OracleParameter
                        {
                            ParameterName = ":id",
                            DbType = System.Data.DbType.Decimal,
                            Direction = System.Data.ParameterDirection.Output
                        });

                        command.ExecuteNonQuery();

                        var id = Convert.ToInt64(command.Parameters[":id"].Value).ToString();

                        return id.ToString();
                    }
                }
            }
        }

        public override void UpdateVersion(string versionId, Core.Db.Version version)
        {
            lock (padlock)
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("UPDATE " + Core.Db.Version.DocumentName + " SET "
                        + Version.ColumnName_RecordId + " = " + int.Parse(version.RecordId) + ", "
                        + Version.ColumnName_FilePath + " = '" + (version.FilePath ?? "").Replace("'", "''") + "'"
                        + " WHERE "
                        + Version.ColumnName_Id + " = " + int.Parse(versionId)
                        , conn))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public override void DeleteVersions(string[] versionIds)
        {
            lock (padlock)
            {
                if (versionIds.Length > 0)
                {
                    using (var conn = new OracleConnection(connectionString))
                    {
                        conn.Open();

                        var builder = new StringBuilder("(");

                        for (int i = 0; i < versionIds.Length; i++)
                        {
                            var id = versionIds[i];
                            builder.Append(id);
                            if (i < versionIds.Length - 1)
                            {
                                builder.Append(", ");
                            }
                            else
                            {
                                builder.Append(")");
                            }
                        }

                        using (var command = new OracleCommand("DELETE FROM " + Core.Db.Version.DocumentName
                            + " WHERE " + Version.ColumnName_Id + " IN " + builder.ToString(), conn))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        public override IEnumerable<Core.Db.Version> GetVersions(string recordId)
        {
            lock (padlock)
            {
                List<Version> versions = new List<Version>();

                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("SELECT "
                        + Version.ColumnName_Id + ", "
                        + Version.ColumnName_RecordId + ", "
                        + Version.ColumnName_FilePath + ", "
                        + Version.ColumnName_CreatedOn
                        + " FROM " + Core.Db.Version.DocumentName
                        + " WHERE " + Version.ColumnName_RecordId + " = " + int.Parse(recordId)
                        , conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var version = new Version
                                {
                                    Id = Convert.ToInt64((decimal)reader[Version.ColumnName_Id]),
                                    RecordId = Convert.ToInt64((decimal)reader[Version.ColumnName_RecordId]).ToString(),
                                    FilePath = reader[Version.ColumnName_FilePath] == DBNull.Value ? null : (string)reader[Version.ColumnName_FilePath],
                                    CreatedOn = (DateTime)reader[Version.ColumnName_CreatedOn]
                                };

                                versions.Add(version);
                            }
                        }
                    }
                }

                return versions;
            }
        }

        public override Core.Db.Version GetLatestVersion(string recordId)
        {
            lock (padlock)
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("SELECT "
                        + Version.ColumnName_Id + ", "
                        + Version.ColumnName_RecordId + ", "
                        + Version.ColumnName_FilePath + ", "
                        + Version.ColumnName_CreatedOn
                        + " FROM " + Core.Db.Version.DocumentName
                        + " WHERE " + Version.ColumnName_RecordId + " = " + int.Parse(recordId)
                        + " ORDER BY " + Version.ColumnName_CreatedOn + " DESC"
                        + " FETCH NEXT 1 ROWS ONLY", conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var version = new Version
                                {
                                    Id = Convert.ToInt64((decimal)reader[Version.ColumnName_Id]),
                                    RecordId = Convert.ToInt64((decimal)reader[Version.ColumnName_RecordId]).ToString(),
                                    FilePath = reader[Version.ColumnName_FilePath] == DBNull.Value ? null : (string)reader[Version.ColumnName_FilePath],
                                    CreatedOn = (DateTime)reader[Version.ColumnName_CreatedOn]
                                };

                                return version;
                            }
                        }
                    }
                }

                return null;
            }
        }

        public override string InsertNotification(Core.Db.Notification notification)
        {
            lock (padlock)
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("INSERT INTO " + Core.Db.Notification.DocumentName + "("
                        + Notification.ColumnName_AssignedBy + ", "
                        + Notification.ColumnName_AssignedOn + ", "
                        + Notification.ColumnName_AssignedTo + ", "
                        + Notification.ColumnName_Message + ", "
                        + Notification.ColumnName_IsRead + ")"
                        + " VALUES("
                        + (!string.IsNullOrEmpty(notification.AssignedBy) ? int.Parse(notification.AssignedBy).ToString() : "NULL") + ", "
                        + "TO_TIMESTAMP(" + "'" + notification.AssignedOn.ToString(dateTimeFormat) + "'" + ", 'YYYY-MM-DD HH24:MI:SS.FF')" + ", "
                        + (!string.IsNullOrEmpty(notification.AssignedTo) ? int.Parse(notification.AssignedTo).ToString() : "NULL") + ", "
                        + "'" + (notification.Message ?? "").Replace("'", "''") + "'" + ", "
                        + (notification.IsRead ? "1" : "0") + ")"
                        + " RETURNING " + Notification.ColumnName_Id + " INTO :id"
                        , conn))
                    {
                        command.Parameters.Add(new OracleParameter
                        {
                            ParameterName = ":id",
                            DbType = System.Data.DbType.Decimal,
                            Direction = System.Data.ParameterDirection.Output
                        });

                        command.ExecuteNonQuery();

                        var id = Convert.ToInt64(command.Parameters[":id"].Value).ToString();

                        return id.ToString();
                    }
                }
            }
        }

        public override void MarkNotificationsAsRead(string[] notificationIds)
        {
            lock (padlock)
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    var builder = new StringBuilder("(");

                    for (int i = 0; i < notificationIds.Length; i++)
                    {
                        var id = notificationIds[i];
                        builder.Append(id);
                        if (i < notificationIds.Length - 1)
                        {
                            builder.Append(", ");
                        }
                        else
                        {
                            builder.Append(")");
                        }
                    }

                    using (var command = new OracleCommand("UPDATE " + Core.Db.Notification.DocumentName
                        + " SET " + Notification.ColumnName_IsRead + " = " + "1"
                        + " WHERE " + Notification.ColumnName_Id + " IN " + builder.ToString(), conn))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public override void MarkNotificationsAsUnread(string[] notificationIds)
        {
            lock (padlock)
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    var builder = new StringBuilder("(");

                    for (int i = 0; i < notificationIds.Length; i++)
                    {
                        var id = notificationIds[i];
                        builder.Append(id);
                        if (i < notificationIds.Length - 1)
                        {
                            builder.Append(", ");
                        }
                        else
                        {
                            builder.Append(")");
                        }
                    }

                    using (var command = new OracleCommand("UPDATE " + Core.Db.Notification.DocumentName
                        + " SET " + Notification.ColumnName_IsRead + " = " + "0"
                        + " WHERE " + Notification.ColumnName_Id + " IN " + builder.ToString(), conn))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public override void DeleteNotifications(string[] notificationIds)
        {
            lock (padlock)
            {
                if (notificationIds.Length > 0)
                {
                    using (var conn = new OracleConnection(connectionString))
                    {
                        conn.Open();

                        var builder = new StringBuilder("(");

                        for (int i = 0; i < notificationIds.Length; i++)
                        {
                            var id = notificationIds[i];
                            builder.Append(id);
                            if (i < notificationIds.Length - 1)
                            {
                                builder.Append(", ");
                            }
                            else
                            {
                                builder.Append(")");
                            }
                        }

                        using (var command = new OracleCommand("DELETE FROM " + Core.Db.Notification.DocumentName
                            + " WHERE " + Notification.ColumnName_Id + " IN " + builder.ToString(), conn))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        public override IEnumerable<Core.Db.Notification> GetNotifications(string assignedTo, string keyword)
        {
            lock (padlock)
            {
                List<Notification> notifications = new List<Notification>();

                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("SELECT "
                        + Notification.ColumnName_Id + ", "
                        + Notification.ColumnName_AssignedBy + ", "
                        + Notification.ColumnName_AssignedOn + ", "
                        + Notification.ColumnName_AssignedTo + ", "
                        + Notification.ColumnName_Message + ", "
                        + Notification.ColumnName_IsRead
                        + " FROM " + Core.Db.Notification.DocumentName
                        + " WHERE " + "(LOWER(" + Notification.ColumnName_Message + ")" + " LIKE '%" + (keyword ?? "").Replace("'", "''").ToLower() + "%'"
                        + " AND " + Notification.ColumnName_AssignedTo + " = " + int.Parse(assignedTo) + ")"
                        + " ORDER BY " + Notification.ColumnName_AssignedOn + " DESC"
                        , conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var notification = new Notification
                                {
                                    Id = Convert.ToInt64((decimal)reader[Notification.ColumnName_Id]),
                                    AssignedBy = Convert.ToInt64((decimal)reader[Notification.ColumnName_AssignedBy]).ToString(),
                                    AssignedOn = (DateTime)reader[Notification.ColumnName_AssignedOn],
                                    AssignedTo = Convert.ToInt64((decimal)reader[Notification.ColumnName_AssignedTo]).ToString(),
                                    Message = reader[Notification.ColumnName_Message] == DBNull.Value ? null : (string)reader[Notification.ColumnName_Message],
                                    IsRead = ((short)reader[Notification.ColumnName_IsRead]) == 1 ? true : false
                                };

                                notifications.Add(notification);
                            }
                        }
                    }
                }

                return notifications;
            }
        }

        public override bool HasNotifications(string assignedTo)
        {
            lock (padlock)
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("SELECT COUNT(*)"
                        + " FROM " + Core.Db.Notification.DocumentName
                        + " WHERE (" + Notification.ColumnName_AssignedTo + " = " + int.Parse(assignedTo)
                        + " AND " + Notification.ColumnName_IsRead + " = " + "0" + ")"
                        , conn))
                    {
                        var count = Convert.ToInt64((decimal)command.ExecuteScalar());
                        var hasNotifications = count > 0;
                        return hasNotifications;
                    }
                }
            }
        }

        public override string InsertApprover(Core.Db.Approver approver)
        {
            lock (padlock)
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("INSERT INTO " + Core.Db.Approver.DocumentName + "("
                        + Approver.ColumnName_UserId + ", "
                        + Approver.ColumnName_RecordId + ", "
                        + Approver.ColumnName_Approved + ", "
                        + Approver.ColumnName_ApprovedOn + ") VALUES("
                        + int.Parse(approver.UserId) + ", "
                        + int.Parse(approver.RecordId) + ", "
                        + (approver.Approved ? "1" : "0") + ", "
                        + (approver.ApprovedOn == null ? "NULL" : "TO_TIMESTAMP(" + "'" + approver.ApprovedOn.Value.ToString(dateTimeFormat) + "'" + ", 'YYYY-MM-DD HH24:MI:SS.FF')") + ") "
                        + "RETURNING " + Approver.ColumnName_Id + " INTO :id"
                        , conn))
                    {
                        command.Parameters.Add(new OracleParameter
                        {
                            ParameterName = ":id",
                            DbType = System.Data.DbType.Decimal,
                            Direction = System.Data.ParameterDirection.Output
                        });

                        command.ExecuteNonQuery();

                        var id = Convert.ToInt64(command.Parameters[":id"].Value).ToString();

                        return id.ToString();
                    }
                }
            }
        }

        public override void UpdateApprover(string approverId, Core.Db.Approver approver)
        {
            lock (padlock)
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("UPDATE " + Core.Db.Approver.DocumentName + " SET "
                        + Approver.ColumnName_UserId + " = " + int.Parse(approver.UserId) + ", "
                        + Approver.ColumnName_RecordId + " = " + int.Parse(approver.RecordId) + ", "
                        + Approver.ColumnName_Approved + " = " + (approver.Approved ? "1" : "0") + ", "
                        + Approver.ColumnName_ApprovedOn + " = " + (approver.ApprovedOn == null ? "NULL" : "TO_TIMESTAMP(" + "'" + approver.ApprovedOn.Value.ToString(dateTimeFormat) + "'" + ", 'YYYY-MM-DD HH24:MI:SS.FF')")
                        + " WHERE "
                        + Approver.ColumnName_Id + " = " + int.Parse(approverId)
                        , conn))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public override void DeleteApproversByRecordId(string recordId)
        {
            lock (padlock)
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("DELETE FROM " + Core.Db.Approver.DocumentName
                        + " WHERE " + Approver.ColumnName_RecordId + " = " + int.Parse(recordId), conn))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public override void DeleteApprovedApprovers(string recordId)
        {
            lock (padlock)
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("DELETE FROM " + Core.Db.Approver.DocumentName
                        + " WHERE " + Approver.ColumnName_RecordId + " = " + int.Parse(recordId)
                        + " AND " + Approver.ColumnName_Approved + " = " + "1"
                        , conn))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public override void DeleteApproversByUserId(string userId)
        {
            lock (padlock)
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("DELETE FROM " + Core.Db.Approver.DocumentName
                        + " WHERE " + Approver.ColumnName_UserId + " = " + int.Parse(userId), conn))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public override IEnumerable<Core.Db.Approver> GetApprovers(string recordId)
        {
            lock (padlock)
            {
                List<Approver> approvers = new List<Approver>();

                using (var conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    using (var command = new OracleCommand("SELECT "
                        + Approver.ColumnName_Id + ", "
                        + Approver.ColumnName_UserId + ", "
                        + Approver.ColumnName_RecordId + ", "
                        + Approver.ColumnName_Approved + ", "
                        + Approver.ColumnName_ApprovedOn
                        + " FROM " + Core.Db.Approver.DocumentName
                        + " WHERE " + Approver.ColumnName_RecordId + " = " + int.Parse(recordId)
                        , conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var approver = new Approver
                                {
                                    Id = Convert.ToInt64((decimal)reader[Approver.ColumnName_Id]),
                                    UserId = Convert.ToInt64((decimal)reader[Approver.ColumnName_UserId]).ToString(),
                                    RecordId = Convert.ToInt64((decimal)reader[Approver.ColumnName_RecordId]).ToString(),
                                    Approved = (short)reader[Approver.ColumnName_Approved] == 1 ? true : false,
                                    ApprovedOn = reader[Approver.ColumnName_ApprovedOn] == DBNull.Value ? null : (DateTime?)reader[Approver.ColumnName_ApprovedOn]
                                };

                                approvers.Add(approver);
                            }
                        }
                    }
                }

                return approvers;
            }
        }

        public override void Dispose()
        {
        }
    }
}
