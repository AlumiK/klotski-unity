﻿using System;
using System.Data;
using Mono.Data.Sqlite;
using UnityEngine;

namespace Common
{
    public class Database
    {
        private IDbConnection mCon;

        public Database()
        {
            OpenDb("save.db");
        }

        private void OpenDb(string db)
        {
            var connection = "URI=file:" + Application.persistentDataPath + "/" + db;
            mCon = new SqliteConnection(connection);
            mCon.Open();
            var dbCmd = mCon.CreateCommand();
            dbCmd.CommandText =
                "create table if not exists stage_progress (id integer primary key, best_steps integer, best_time integer)";
            dbCmd.ExecuteNonQuery();
            dbCmd.CommandText =
                "create table if not exists configs (id integer primary key, value integer)";
            dbCmd.ExecuteNonQuery();
            dbCmd.CommandText = "SELECT count(*) FROM configs";
            var count = Convert.ToInt32(dbCmd.ExecuteScalar());
            if (count == 0)
            {
                dbCmd.CommandText = "INSERT INTO configs VALUES (1, 0); INSERT INTO configs VALUES (2, 1)";
                dbCmd.ExecuteNonQuery();
            }

            mCon.Close();
        }

        public bool SaveResult(int stageId, int steps, int time)
        {
            mCon.Open();
            var dbCmd = mCon.CreateCommand();
            dbCmd.CommandText = "SELECT count(*) FROM stage_progress WHERE id=@stageId";
            dbCmd.Parameters.Add(new SqliteParameter("@stageId", stageId));
            var newRecord = false;
            if (Convert.ToInt32(dbCmd.ExecuteScalar()) == 0)
            {
                dbCmd.CommandText = "INSERT INTO stage_progress VALUES (@stageId, @steps, @time)";
                dbCmd.Parameters.Add(new SqliteParameter("@stageId", stageId));
                dbCmd.Parameters.Add(new SqliteParameter("@steps", steps));
                dbCmd.Parameters.Add(new SqliteParameter("@time", time));
                dbCmd.ExecuteNonQuery();
                newRecord = true;
            }
            else
            {
                dbCmd.CommandText = "SELECT best_steps, best_time FROM stage_progress WHERE id=@stageId";
                dbCmd.Parameters.Add(new SqliteParameter("@stageId", stageId));
                var reader = dbCmd.ExecuteReader();
                reader.Read();
                var bestSteps = Convert.ToInt32(reader[0]);
                var bestTime = Convert.ToInt32(reader[1]);
                reader.Close();
                
                if (bestSteps > steps)
                {
                    dbCmd.CommandText = "UPDATE stage_progress SET best_steps=@steps WHERE id=@stageId";
                    dbCmd.Parameters.Add(new SqliteParameter("@stageId", stageId));
                    dbCmd.Parameters.Add(new SqliteParameter("@steps", steps));
                    dbCmd.ExecuteNonQuery();
                    newRecord = true;
                }

                if (bestTime > time)
                {
                    dbCmd.CommandText = "UPDATE stage_progress SET best_time=@time WHERE id=@stageId";
                    dbCmd.Parameters.Add(new SqliteParameter("@stageId", stageId));
                    dbCmd.Parameters.Add(new SqliteParameter("@time", time));
                    dbCmd.ExecuteNonQuery();
                }
            }

            mCon.Close();
            return newRecord;
        }

        public struct Result
        {
            public bool Finished;
            public int BestTime;
            public int BestSteps;
        }

        public bool GetFinished(int stageId)
        {
            mCon.Open();
            var dbCmd = mCon.CreateCommand();
            dbCmd.CommandText = "SELECT count(*) FROM stage_progress WHERE id=@stageId";
            dbCmd.Parameters.Add(new SqliteParameter("@stageId", stageId));
            var result = Convert.ToInt32(dbCmd.ExecuteScalar());
            mCon.Close();
            return result != 0;
        }

        public Result GetResult(int stageId)
        {
            mCon.Open();
            var result = new Result();
            var dbCmd = mCon.CreateCommand();
            dbCmd.CommandText = "SELECT count(*) FROM stage_progress WHERE id=@stageId";
            dbCmd.Parameters.Add(new SqliteParameter("@stageId", stageId));
            if (Convert.ToInt32(dbCmd.ExecuteScalar()) == 0)
            {
                result.Finished = false;
            }
            else
            {
                result.Finished = true;
                dbCmd.CommandText = "SELECT best_steps, best_time FROM stage_progress WHERE id=@stageId";
                dbCmd.Parameters.Add(new SqliteParameter("@stageId", stageId));
                var reader = dbCmd.ExecuteReader();
                reader.Read();
                result.BestSteps = Convert.ToInt32(reader[0]);
                result.BestTime = Convert.ToInt32(reader[1]);
                reader.Close();
            }

            mCon.Close();
            return result;
        }

        public int GetConfig(int id)
        {
            mCon.Open();
            var dbCmd = mCon.CreateCommand();
            dbCmd.CommandText = "SELECT value FROM configs WHERE id=@id";
            dbCmd.Parameters.Add(new SqliteParameter("@id", id));
            var result = Convert.ToInt32(dbCmd.ExecuteScalar());
            mCon.Close();
            return result;
        }

        public void SetConfig(int id, int value)
        {
            mCon.Open();
            var dbCmd = mCon.CreateCommand();
            dbCmd.CommandText = "UPDATE configs SET value=@value WHERE id=@id";
            dbCmd.Parameters.Add(new SqliteParameter("@value", value));
            dbCmd.Parameters.Add(new SqliteParameter("@id", id));
            dbCmd.ExecuteNonQuery();
            mCon.Close();
        }
    }
}