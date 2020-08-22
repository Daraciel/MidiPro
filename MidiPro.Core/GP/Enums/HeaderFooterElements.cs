namespace MidiPro.Core.GP.Enums
{
    public enum HeaderFooterElements
    {
        None = 0x000,
        Title = 0x001,
        Subtitle = 0x002,
        Artist = 0x004,
        Album = 0x008,
        Words = 0x010,
        Music = 0x020,
        WordsAndMusic = 0x040,
        Copyright = 0x080,
        PageNumber = 0x100,
        All = Title | Subtitle | Artist | Album | Words | Music | WordsAndMusic | Copyright | PageNumber
    }
}