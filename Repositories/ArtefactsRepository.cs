using Google.Cloud.Firestore;
using System.Reflection.Metadata;
using ZachPerini_6._3A_HA.Models;

namespace ZachPerini_6._3A_HA.Repositories
{
    public class ArtefactsRepository
    {
        private FirestoreDb db;
        public ArtefactsRepository(string project)
        {
            db = FirestoreDb.Create(project);
        }

        //insert new artefact in database in no-sql (firestore) database
        public async void AddArtefact(Artefact a)
        {
            DocumentReference docRef = db.Collection("artefacts").Document(a.Id);
            await docRef.SetAsync(a);
        }

        public async Task<List<Artefact>> GetArtefacts()
        {
            List<Artefact> artefacts = new List<Artefact>();
            Query artefactsQuery = db.Collection("artefacts");
            QuerySnapshot artefactsQuerySnapshot = await artefactsQuery.GetSnapshotAsync();
            foreach (DocumentSnapshot documentSnapshot in artefactsQuerySnapshot.Documents)
            {
                Artefact artefact = documentSnapshot.ConvertTo<Artefact>();
                artefact.Id = documentSnapshot.Id;//assign the id used for the artefact within the no-sql database
                artefacts.Add(artefact);
            }

            return artefacts;
        }

        public async Task<WriteResult> DeleteArtefact(string Id)
        {
            DocumentReference artefactsRef = db.Collection("artefacts").Document(Id); 
            return await artefactsRef.DeleteAsync();
        }

        public async Task<WriteResult> ChangeStatus(Artefact artefact)
        {
            //in assignment i need to do similar thing automatically to change status from 0 to 1
            DocumentReference artefactRef = db.Collection("artefacts").Document(artefact.Id);
            Dictionary<string, object> updates = new Dictionary<string, object>
            {
                { "status",  "1"},
            };
            return await artefactRef.UpdateAsync(updates);
        }
    }
}
