using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Hosting;
using ZachPerini_6._3A_HA.Models;

namespace ZachPerini_6._3A_HA.Repositories
{
    public class SharedUserRepository
    {
        private FirestoreDb db;
        public SharedUserRepository(string project)
        {
            db = FirestoreDb.Create(project);
        }

        public async void AddUser(SharedUser user)
        {
            DocumentReference docRef = db.Collection($"artefacts/{user.Artefact_Id}/shared_users").Document(user.Id);
            await docRef.SetAsync(user);
        }


        public async Task<List<Artefact>> GetSharedArtefactsForUser(string userEmail)
        {
            List<Artefact> sharedArtefacts = new List<Artefact>();

            try
            {
                // Query shared_users collection for the user's email
                QuerySnapshot sharedUsersSnapshot = await db.CollectionGroup("shared_users")
                                                            .WhereEqualTo("Email", userEmail)
                                                            .GetSnapshotAsync();

                // Iterate over shared user documents
                foreach (DocumentSnapshot sharedUserSnapshot in sharedUsersSnapshot.Documents)
                {
                    // Get the parent artefact document ID
                    string artefactId = sharedUserSnapshot.Reference.Parent.Parent.Id;

                    // Retrieve the artefact document using its ID
                    DocumentSnapshot artefactSnapshot = await db.Collection("artefacts")
                                                                 .Document(artefactId)
                                                                 .GetSnapshotAsync();

                    // Convert the artefact document to an Artefact object
                    Artefact artefact = artefactSnapshot.ConvertTo<Artefact>();
                    if (artefact != null)
                    {
                        artefact.Id = artefactId; // Assign the artefact ID
                        sharedArtefacts.Add(artefact);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving shared artefacts: {ex.Message}");
                // Handle the exception if necessary
            }

            return sharedArtefacts;
        }

        //Get all users
        public async Task<List<SharedUser>> GetUsers(string artefactId)
        {
            List<SharedUser> usersList = new List<SharedUser>();
            Query postsQuery = db.Collection($"artefacts/{artefactId}/shared_users");
            QuerySnapshot postsQuerySnapshot = await postsQuery.GetSnapshotAsync();
            foreach (DocumentSnapshot documentSnapshot in postsQuerySnapshot.Documents)
            {
                SharedUser user = documentSnapshot.ConvertTo<SharedUser>();
                user.Id = documentSnapshot.Id;//assign the id used for the blog within the no-sql database
                user.Artefact_Id = artefactId;
                usersList.Add(user);
            }

            return usersList;
        }

        // Get only one user
        public async Task<SharedUser> GetUser(string artefactId, string userId)
        {
            var docRef = await db.Collection($"artefacts/{artefactId}/shared_users").Document(userId).GetSnapshotAsync();

            if (docRef == null)
            {
                return null;
            }

            SharedUser user = docRef.ConvertTo<SharedUser>();
            user.Id = docRef.Id;//assign the id used for the blog within the no-sql database
            user.Artefact_Id = artefactId;
            return user;
        }
    }
}
