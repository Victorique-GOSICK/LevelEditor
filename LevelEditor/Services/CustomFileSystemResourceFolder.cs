﻿//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

using Sce.Atf;

using LevelEditorCore;

namespace LevelEditor
{
    public class CustomFileSystemResourceFolder : IFileSystemResourceFolder
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="path">Absolute path of the directory</param>
        public CustomFileSystemResourceFolder(string path)
            : this(path, null)
        {
        }

        private CustomFileSystemResourceFolder(string path, IResourceFolder parent)
        {
            m_path = path;
            m_parent = parent;
            m_name = PathUtil.GetLastElement(path);
        }

        #region IResourceFolder Members

        /// <summary>
        /// Gets a list of subfolders, if any. If there are no subfolders an empty list is returned.
        /// This IList will be read-only.</summary>
        public IList<IResourceFolder> Folders
        {
            get
            {
                string[] directories = null;

                try
                {
                    // This can throw!
                    directories = Directory.GetDirectories(m_path);
                }
                catch (UnauthorizedAccessException) {}
                catch (ArgumentNullException) {}
                catch (ArgumentException) {}
                catch (PathTooLongException) {}
                catch (DirectoryNotFoundException) {}
                catch (IOException) {}
                finally
                {
                    if (directories == null)
                        directories = new string[0];
                }

                List<IResourceFolder> folders = new List<IResourceFolder>(directories.Length);
                foreach (string directory in directories)
                    folders.Add(new CustomFileSystemResourceFolder(directory, this));
                return new ReadOnlyCollection<IResourceFolder>(folders);
            }
        }

        /// <summary>
        /// Gets a list of Resource URIs contained in this folder. If none, an empty list is returned.
        /// This IList will be read-only.</summary>
        public IList<Uri> ResourceUris
        {
            get
            {
                string[] files = null;

                try
                {
                    // This can throw!
                    files = Directory.GetFiles(m_path);
                }
                catch (UnauthorizedAccessException) { }
                catch (ArgumentNullException) { }
                catch (ArgumentException) { }
                catch (PathTooLongException) { }
                catch (DirectoryNotFoundException) { }
                catch (IOException) { }
                finally
                {
                    if (files == null)
                        files = new string[0];
                }


                IResourceMetadataService resService
                    = Globals.MEFContainer.GetExportedValue<IResourceMetadataService>();
                IEnumerable<string> metaExts = resService != null ? resService.MetadataFileExtensions
                    : EmptyEnumerable<string>.Instance;

                List<Uri> uris = new List<Uri>(files.Length);
                foreach (string file in files)
                {
                    // skip over metadata and thumbnais.
                    string ext = System.IO.Path.GetExtension(file);
                    string fileName = System.IO.Path.GetFileName(file);
                    bool skip = fileName.StartsWith("~") || string.IsNullOrWhiteSpace(ext);
                    if(!skip)
                    {
                        foreach(string metaExt in metaExts)
                        {
                            if (ext.Equals(metaExt, StringComparison.InvariantCultureIgnoreCase))
                            {
                                skip = true;
                                break;
                            }
                        }
                    }
                    if(skip) continue;

                    uris.Add(new Uri(file));
                }                                           
                return new ReadOnlyCollection<Uri>(uris);
            }
        }

        /// <summary>
        /// Gets the parent folder. Returns null if the current folder is the root of a folder tree</summary>
        public IResourceFolder Parent
        {
            get { return m_parent; }
        }

        /// <summary>
        /// Gets whether the Name property is read-only. If true, setting the Name may not have any
        /// effect.</summary>
        public bool ReadOnlyName
        {
            get { return true; }
        }

        /// <summary>
        /// Gets or sets the display name of the folder. The initial value is the directory name. Setting
        /// the name does not modify the corresponding directory name.</summary>
        public virtual string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        /// <summary>
        /// Not implemented. Returns null.</summary>
        /// <returns>Null</returns>
        public virtual IResourceFolder CreateFolder()
        {
            return null;
        }

        #endregion

        /// <summary>
        /// Gets the absolute path of the directory that this IResourceFolder maps to</summary>
        public string FullPath
        {
            get { return m_path; }
        }

        private string m_name;
        private readonly string m_path;
        private readonly IResourceFolder m_parent;
    }
}
