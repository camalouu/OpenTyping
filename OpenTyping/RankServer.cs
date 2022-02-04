﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace OpenTyping
{
    public class RankServer : IRank
    {
        private List<User> users;
        private readonly FirestoreProvider Firestore = new FirestoreProvider();

        public RankServer() { }

        public async Task<List<User>> GetUsersAsync()
        {
            try
            {
                if (Firestore.OpenDatabase())
                {
                    users = (List<User>)await Firestore.GetUsersAsync<User>();
                    if (users.Count == 0) // If first insertion
                    {
                        users = new List<User>();
                    }
                    users.Sort();

                    return users;
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Unavailable"))
                {
                    Debug.WriteLine("Failed to get users from Server. Network is unavailable!");
                }
            }

            return null;
        }

        public async Task<int> AddSync(User user)
        {
            users.Add(user);
            users.Sort();

            int curPos = users.FindIndex(user.Equals);
            if (curPos == 10) // If not new record
            {
                users.RemoveAt(users.Count - 1);
                return -1;
            }

            if (users.Count > 10) // If records are already full
            {
                users.RemoveAt(users.Count - 1); // Remove always 11th last record
            }

            try
            {
                await Firestore.AddAsync<User>(user);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Unavailable"))
                {
                    Debug.WriteLine("Failed to add to Server. Network is unavailable!");
                }
            }
            
            return curPos;
        }
    }
}