using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace AssCS
{
    /// <summary>
    /// Manages the styles in a file
    /// </summary>
    public class StyleManager
    {
        private readonly ObservableCollection<Style> _styles;
        public ObservableCollection<Style> Styles => _styles;

        private int _id = 0;
        public int NextId => _id++;

        public int Add(Style s)
        {
            _styles.Add(s);
            return s.Id;
        }

        public int AddOrReplace(Style s)
        {
            Remove(s.Name);
            _styles.Add(s);
            return s.Id;
        }

        public Style? Get(string name)
        {
            return _styles.Where(s => s.Name == name).FirstOrDefault();
        }

        public Style? Get(int id)
        {
            return _styles.Where(s => s.Id == id).FirstOrDefault();
        }

        public bool Remove(string name)
        {
            bool removed = false;
            foreach (var s in _styles.Where(s => s.Name == name).ToList())
            {
                removed = _styles.Remove(s);
            }
            return removed;
        }

        public bool Remove(int id)
        {
            bool removed = false;
            foreach (var s in _styles.Where(s => s.Id == id))
            {
                removed = _styles.Remove(s);
            }
            return removed;
        }

        public void Clear()
        {
            _styles.Clear();
        }

        public List<Style> GetAll()
        {
            return _styles.ToList();
        }

        public void LoadDefault()
        {
            Clear();
            Add(new Style(NextId));
        }

        public StyleManager(StyleManager source)
        {
            _styles = new ObservableCollection<Style>(source._styles);
        }

        public StyleManager(File source)
        {
            _styles = new ObservableCollection<Style>(source.StyleManager._styles);
        }

        public StyleManager()
        {
            _styles = new ObservableCollection<Style>();
        }
    }
}
