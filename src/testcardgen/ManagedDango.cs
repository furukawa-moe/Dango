using Python.Runtime;

namespace testcardgen
{
    public class ManagedDango
    {
        public ManagedDango()
        {
            Runtime.PythonDLL = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\..\\Local\\Programs\\Python\\Python311\\python311.dll";
            PythonEngine.Initialize();
        }

        public List<DangoWord> Tokenize(string input)
        {
            if(string.IsNullOrEmpty(input)) { throw new ArgumentNullException("input"); }

            List<DangoWord> tokens = new List<DangoWord>();

            using (Py.GIL())
            {
                dynamic _ = Py.Import("builtins");
                dynamic dango = Py.Import("dango");
                dynamic words = dango.tokenize(input);

                for (int i = 0; i < (int)_.len(words); i++)
                {
					DangoWord word = new DangoWord()
                    {
                        PartOfSpeech = words[i].part_of_speech.name,
                        Surface = words[i].surface,
                        SurfaceReading = words[i].surface_reading,
                        DictionaryForm = words[i].dictionary_form,
                        DictionaryFormReading = words[i].dictionary_form_reading,
                    };

                    tokens.Add(word);
                    
                }
            }

            return tokens;
        }
    }

    public class DangoWord
    {
        // todo: create an enum for different PartsOfSpeech
        public DangoWord() { }
        public override string ToString()
        {
            return $"-- Word\nPartOfSpeech {PartOfSpeech}\nSurface {Surface}\nSurfaceReading {SurfaceReading}\nDictionaryForm {DictionaryForm}\nDictionaryFormReading {DictionaryFormReading}";
        }
        internal string PartOfSpeech { get; set; } = "";
        internal string Surface { get; set; } = "";
        internal string SurfaceReading { get; set; } = "";
        internal string DictionaryForm { get; set; } = "";
        internal string DictionaryFormReading { get; set; } = "";
    }
}
