using Google.Cloud.Firestore;

namespace ZachPerini_6._3A_HA.Models
{
    [FirestoreData]
    public class Artefact
    {
        public string Id { get; set; }

        [FirestoreProperty]
        public string Title { get; set; }

        [FirestoreProperty]
        public Timestamp DateUploaded { get; set; }

        [FirestoreProperty]
        public string Author { get; set; }

        [FirestoreProperty]
        public string file { get; set; }

        [FirestoreProperty]
        public string status { get; set; }
    }
}
