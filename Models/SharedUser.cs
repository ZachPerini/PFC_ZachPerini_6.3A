using Google.Cloud.Firestore;

namespace ZachPerini_6._3A_HA.Models
{
    [FirestoreData]
    public class SharedUser
    {
        public string Id { get; set; }
        [FirestoreProperty]
        public string Email { get; set; }

        public string Artefact_Id { get; set; }
    }
}
