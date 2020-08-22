using MidiPro.Core.BE;
using MidiPro.Core.GP.Enums;

namespace MidiPro.Core.GP
{
    public class PageSetup
    {
        /*The page setup describes how the document is rendered.

            Page setup contains page size, margins, paddings, and how the title
            elements are rendered.

            Following template vars are available for defining the page texts:

            - ``%title%``: will be replaced with Song.title
            - ``%subtitle%``: will be replaced with Song.subtitle
            - ``%artist%``: will be replaced with Song.artist
            - ``%album%``: will be replaced with Song.album
            - ``%words%``: will be replaced with Song.words
            - ``%music%``: will be replaced with Song.music
            - ``%WORDSANDMUSIC%``: will be replaced with the according word
              and music values
            - ``%copyright%``: will be replaced with Song.copyright
            - ``%N%``: will be replaced with the current page number (if
              supported by layout)
            - ``%P%``: will be replaced with the number of pages (if supported
              by layout)*/
        public Point2D PageSize { get; set; }
        public Padding PageMargin { get; set; }
        public float ScoreSizeProportion { get; set; }
        public HeaderFooterElements HeaderAndFooter { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Words { get; set; }
        public string Music { get; set; }
        public string WordsAndMusic { get; set; }
        public string Copyright { get; set; }
        public string PageNumber { get; set; }

        public PageSetup()
        {
            PageSize = new Point2D(210, 297);
            PageMargin = new Padding(10, 15, 10, 10);
            ScoreSizeProportion = 1.0f;
            HeaderAndFooter = HeaderFooterElements.All;
            Title = "%title%";
            Subtitle = "%subtitle%";
            Artist = "%artist%";
            Album = "%album%";
            Words = "Words by %words%";
            Music = "Music by %music%";
            WordsAndMusic = "Words & Music by %WORDSMUSIC%";
            Copyright = "Copyright %copyright%\nAll Rights Reserved - International Copyright Secured";
            PageNumber = "Page %N%/%P%";
        }
    }
}