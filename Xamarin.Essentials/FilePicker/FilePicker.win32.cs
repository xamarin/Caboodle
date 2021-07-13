using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class FilePicker
    {
        static Task<IEnumerable<FileResult>> PlatformPickAsync(PickOptions options, bool allowMultiple = false)
        {
            var fod = new NativeMethods.FileOpenDialog() as NativeMethods.IFileOpenDialog;

            SetFileTypes(options, fod);
            fod.SetTitle(options.PickerTitle);
            fod.SetOptions(allowMultiple ? NativeMethods.FILEOPENDIALOGOPTIONS.FOS_ALLOWMULTISELECT : 0);

            var showResult = fod.Show(IntPtr.Zero);

            if (showResult < 0)
                return Task.FromResult<IEnumerable<FileResult>>(new List<FileResult>());

            if (allowMultiple)
            {
                fod.GetResults(out var items);
                items.GetCount(out var count);
                var results = new List<FileResult>();
                for (var i = 0; i < count; i++)
                {
                    items.GetItemAt(i, out var item);
                    item.GetDisplayName(NativeMethods.SIGDN.SIGDN_FILESYSPATH, out var path);
                    results.Add(new FileResult(path));
                }

                return Task.FromResult<IEnumerable<FileResult>>(results);
            }
            else
            {
                fod.GetResult(out var item);
                item.GetDisplayName(NativeMethods.SIGDN.SIGDN_FILESYSPATH, out var filename);
                return Task.FromResult<IEnumerable<FileResult>>(new List<FileResult>() { new FileResult(filename) });
            }
        }

        static void SetFileTypes(PickOptions options, NativeMethods.IFileOpenDialog dialog)
        {
            var hasAtLeastOneType = false;
            var filters = new List<NativeMethods.COMDLG_FILTERSPEC>();

            if (options?.FileTypes == FilePickerFileType.Images)
            {
                var types = new StringBuilder();
                foreach (var type in options.FileTypes.Value)
                {
                    types.Append(type + ";");
                }

                filters.Add(new NativeMethods.COMDLG_FILTERSPEC { pszName = "Images\0", pszSpec = types.ToString() + "\0" });
                hasAtLeastOneType = true;
            }
            else if (options?.FileTypes == FilePickerFileType.Videos)
            {
                var types = new StringBuilder();
                foreach (var type in options.FileTypes.Value)
                {
                    types.Append(type + ";");
                }

                filters.Add(new NativeMethods.COMDLG_FILTERSPEC { pszName = "Videos\0", pszSpec = types.ToString() + "\0" });
                hasAtLeastOneType = true;
            }
            else if (options?.FileTypes?.Value != null)
            {
                foreach (var type in options.FileTypes.Value)
                {
                    if (type.StartsWith(".") || type.StartsWith("*."))
                    {
                        filters.Add(new NativeMethods.COMDLG_FILTERSPEC { pszName = type.TrimStart('*') + "\0", pszSpec = type + "\0" });
                        hasAtLeastOneType = true;
                    }
                }
            }

            if (hasAtLeastOneType)
            {
                dialog.SetFileTypes(filters.Count, filters.ToArray());
            }
        }

        static class NativeMethods
        {
            [ComImport]
            [Guid("DC1C5A9C-E88A-4dde-A5A1-60F82A20AEF7")]
            [ClassInterface(ClassInterfaceType.None)]
            public class FileOpenDialog
            {
            }

            [ComImport]
            [Guid("b4db1657-70d7-485e-8e3e-6fcb5a5c1802")]
            [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
            public interface IModalWindow
            {
                [PreserveSig]
                int Show(IntPtr hwndOwner);
            }

            [ComImport]
            [Guid("42f85136-db7e-439c-85f1-e4075d135fc8")]
            [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
            public interface IFileDialog : IModalWindow
            {
                [PreserveSig]
                new int Show(IntPtr hwndOwner);

                void SetFileTypes(int cFileTypes, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] COMDLG_FILTERSPEC[] rgFilterSpec);

                void SetFileTypeIndex(uint iFileType);

                void GetFileTypeIndex(out uint piFileType);

                void Advise(IntPtr pfde, out uint pdwCookie);

                void Unadvise(uint dwCookie);

                void SetOptions(FILEOPENDIALOGOPTIONS fos);

                void GetOptions(out FILEOPENDIALOGOPTIONS pfos);

                void SetDefaultFolder(IShellItem psi);

                void SetFolder(IShellItem psi);

                void GetFolder(out IShellItem ppsi);

                void GetCurrentSelection(out IShellItem ppsi);

                void SetFileName(string pszName);

                void GetFileName(out string pszName);

                void SetTitle(string pszTitle);

                void SetOkButtonLabel(string pszText);

                void SetFileNameLabel(string pszLabel);

                void GetResult(out IShellItem ppsi);

                void AddPlace(IShellItem psi, FDAP fdap);

                void SetDefaultExtension(string pszDefaultExtension);

                void Close(int hr);

                void SetClientGuid(ref Guid guid);

                void ClearClientData();

                void SetFilter(IntPtr/*IShellItemFilter*/ pFilter);
            }

            [ComImport]
            [Guid("d57c7288-d4ad-4768-be02-9d969532d960")]
            [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
            public interface IFileOpenDialog : IFileDialog
            {
                [PreserveSig]
                new int Show(IntPtr hwndOwner);

                new void SetFileTypes(int cFileTypes, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex =0)] COMDLG_FILTERSPEC[] rgFilterSpec);

                new void SetFileTypeIndex(uint iFileType);

                new void GetFileTypeIndex(out uint piFileType);

                new void Advise(IntPtr pfde, out uint pdwCookie);

                new void Unadvise(uint dwCookie);

                new void SetOptions(FILEOPENDIALOGOPTIONS fos);

                new void GetOptions(out FILEOPENDIALOGOPTIONS pfos);

                new void SetDefaultFolder(IShellItem psi);

                new void SetFolder(IShellItem psi);

                new void GetFolder(out IShellItem ppsi);

                new void GetCurrentSelection(out IShellItem ppsi);

                new void SetFileName(string pszName);

                new void GetFileName(out string pszName);

                new void SetTitle(string pszTitle);

                new void SetOkButtonLabel(string pszText);

                new void SetFileNameLabel(string pszLabel);

                new void GetResult(out IShellItem ppsi);

                new void AddPlace(IShellItem psi, FDAP fdap);

                new void SetDefaultExtension(string pszDefaultExtension);

                new void Close(int hr);

                new void SetClientGuid(ref Guid guid);

                new void ClearClientData();

                new void SetFilter(IntPtr/*IShellItemFilter*/ pFilter);

                void GetResults(out IShellItemArray ppenum);

                void GetSelectedItems(out IShellItemArray ppsai);
            }

            [ComImport]
            [Guid("b63ea76d-1f85-456f-a19c-48159efa858b")]
            [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
            public interface IShellItemArray
            {
                void BindToHandler(
                    IntPtr pbc,
                    ref Guid bhid,
                    ref Guid riid,
                    out IntPtr ppv);

                void GetPropertyStore(int flags, ref Guid riid, out IntPtr ppv);

                void GetPropertyDescriptionList(int keyType, ref Guid riid, out IntPtr ppv);

                void GetAttributes(int AttribFlags, SFGAOF sfgaoMask, out SFGAOF psfgaoAttribs);

                void GetCount(out int pdwNumItems);

                void GetItemAt(int dwIndex, out IShellItem ppsi);

                void EnumItems(out IntPtr ppenumShellItems);
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct COMDLG_FILTERSPEC
            {
                [MarshalAs(UnmanagedType.LPWStr)]
                public string pszName;
                [MarshalAs(UnmanagedType.LPWStr)]
                public string pszSpec;
            }

            public enum FDAP
            {
                FDAP_BOTTOM = 0,
                FDAP_TOP = 1
            }

            [Flags]
            public enum FILEOPENDIALOGOPTIONS : uint
            {
                FOS_OVERWRITEPROMPT = 0x2,
                FOS_STRICTFILETYPES = 0x4,
                FOS_NOCHANGEDIR = 0x8,
                FOS_PICKFOLDERS = 0x20,
                FOS_FORCEFILESYSTEM = 0x40,
                FOS_ALLNONSTORAGEITEMS = 0x80,
                FOS_NOVALIDATE = 0x100,
                FOS_ALLOWMULTISELECT = 0x200,
                FOS_PATHMUSTEXIST = 0x800,
                FOS_FILEMUSTEXIST = 0x1000,
                FOS_CREATEPROMPT = 0x2000,
                FOS_SHAREAWARE = 0x4000,
                FOS_NOREADONLYRETURN = 0x8000,
                FOS_NOTESTFILECREATE = 0x10000,
                FOS_HIDEMRUPLACES = 0x20000,
                FOS_HIDEPINNEDPLACES = 0x40000,
                FOS_NODEREFERENCELINKS = 0x100000,
                FOS_OKBUTTONNEEDSINTERACTION = 0x200000,
                FOS_DONTADDTORECENT = 0x2000000,
                FOS_FORCESHOWHIDDEN = 0x10000000,
                FOS_DEFAULTNOMINIMODE = 0x20000000,
                FOS_FORCEPREVIEWPANEON = 0x40000000,
                FOS_SUPPORTSTREAMABLEITEMS = 0x80000000
            }

            public enum SIGDN : uint
            {
                SIGDN_NORMALDISPLAY = 0,
                SIGDN_PARENTRELATIVEPARSING = 0x80018001,
                SIGDN_DESKTOPABSOLUTEPARSING = 0x80028000,
                SIGDN_PARENTRELATIVEEDITING = 0x80031001,
                SIGDN_DESKTOPABSOLUTEEDITING = 0x8004c000,
                SIGDN_FILESYSPATH = 0x80058000,
                SIGDN_URL = 0x80068000,
                SIGDN_PARENTRELATIVEFORADDRESSBAR = 0x8007c001,
                SIGDN_PARENTRELATIVE = 0x80080001,
                SIGDN_PARENTRELATIVEFORUI = 0x80094001
            }

            public enum SICHINTF : uint
            {
                SICHINT_DISPLAY = 0,
                SICHINT_ALLFIELDS = 0x80000000,
                SICHINT_CANONICAL = 0x10000000,
                SICHINT_TEST_FILESYSPATH_IF_NOT_EQUAL = 0x20000000
            }

            [Flags]
            public enum SFGAOF : uint
            {
                SFGAO_CANCOPY = 0x1, // Objects can be copied    (= 0x1)
                SFGAO_CANMOVE = 0x2, // Objects can be moved     (= 0x2)
                SFGAO_CANLINK = 0x4, // Objects can be ,inked    (= 0x4)
                SFGAO_STORAGE = 0x00000008,     // supports BindToObject(IID_IStorage)
                SFGAO_CANRENAME = 0x00000010,     // Objects can be renamed
                SFGAO_CANDELETE = 0x00000020,     // Objects can be de,eted
                SFGAO_HASPROPSHEET = 0x00000040,     // Objects have property sheets
                SFGAO_DROPTARGET = 0x00000100,     // Objects are drop target
                SFGAO_CAPABILITYMASK = 0x00000177,
                SFGAO_PLACEHOLDER = 0x00000800,     // Fi,e or fo,der is not fu,,y present and reca,,ed on open or access
                SFGAO_SYSTEM = 0x00001000,     // System object
                SFGAO_ENCRYPTED = 0x00002000,     // Object is encrypted (use a,t co,or)
                SFGAO_ISSLOW = 0x00004000,     // 'S,ow' object
                SFGAO_GHOSTED = 0x00008000,     // Ghosted icon
                SFGAO_LINK = 0x00010000,     // Shortcut (link)
                SFGAO_SHARE = 0x00020000,     // Shared
                SFGAO_READONLY = 0x00040000,     // Read-only
                SFGAO_HIDDEN = 0x00080000,     // Hidden object
                SFGAO_DISPLAYATTRMASK = 0x000FC000,
                SFGAO_FILESYSANCESTOR = 0x10000000,     // May contain chi,dren with SFGAO_FILESYSTEM
                SFGAO_FOLDER = 0x20000000,     // Support BindToObject(IID_IShellFolder)
                SFGAO_FILESYSTEM = 0x40000000,     // Is a win32 fi,e system object (file/folder/root)
                SFGAO_HASSUBFOLDER = 0x80000000,     // May contain chi,dren with SFGAO_FO,DER (may be slow)
                SFGAO_CONTENTSMASK = 0x80000000,
                SFGAO_VALIDATE = 0x01000000,     // Inva,idate cached information (may be s,ow)
                SFGAO_REMOVABLE = 0x02000000,     // Is this removeab,e media?
                SFGAO_COMPRESSED = 0x04000000,     // Object is compressed (use a,t co,or)
                SFGAO_BROWSABLE = 0x08000000,     // Supports IShe,,Fo,der, but on,y imp,ements CreateViewObject() (non-fo,der view)
                SFGAO_NONENUMERATED = 0x00100000,     // Is a non-enumerated object (shou,d be hidden)
                SFGAO_NEWCONTENT = 0x00200000,     // Shou,d show bo,d in exp,orer tree
                SFGAO_CANMONIKER = 0x00400000,     // Obso,ete
                SFGAO_HASSTORAGE = 0x00400000,     // Obso,ete
                SFGAO_STREAM = 0x00400000,     // Supports BindToObject(IID_IStream)
                SFGAO_STORAGEANCESTOR = 0x00800000,     // May contain chi,dren with SFGAO_STORAGE or SFGAO_STREAM
                SFGAO_STORAGECAPMASK = 0x70C50008,     // For determining storage capabi,ities, ie for open/save semantics
                SFGAO_PKEYSFGAOMASK = 0x81044000,     // Attributes that are masked out for PKEY_SFGAOF,ags because they are considered to cause s,ow ca,cu,ations or ,ack context (SFGAO_VA,IDATE | SFGAO_ISS,OW | SFGAO_HASSUBFO,DER and others)
            }

            [ComImport]
            [Guid("43826d1e-e718-42ee-bc55-a1e261c37bfe")]
            [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
            public interface IShellItem
            {
                void BindToHandler(
                    IntPtr pbc,
                    ref Guid bhid,
                    ref Guid riid,
                    out IntPtr ppv);

                void GetParent(out IShellItem ppsi);

                void GetDisplayName(SIGDN sigdnName, [MarshalAs(UnmanagedType.LPWStr)] out string ppszName);

                void GetAttributes(SFGAOF sfgaoMask, out SFGAOF psfgaoAttribs);

                void Compare(IShellItem psi, SICHINTF hint, out int piOrder);
            }
        }
    }

    public partial class FilePickerFileType
    {
        static FilePickerFileType PlatformImageFileType() =>
            new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.WindowsDesktop, new[] { "*.png", "*.jpg", "*.jpeg", "*.gif", "*.bmp" } }
            });

        static FilePickerFileType PlatformPngFileType() =>
            new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.WindowsDesktop, new[] { "*.png" } }
            });

        static FilePickerFileType PlatformJpegFileType() =>
            new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.WindowsDesktop, new[] { "*.jpg", "*.jpeg" } }
            });

        static FilePickerFileType PlatformVideoFileType() =>
           new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
           {
                { DevicePlatform.WindowsDesktop, new[] { "*.mp4", "*.mov", "*.avi", "*.wmv", "*.m4v", "*.mpg", "*.mpeg", "*.mp2", "*.mkv", "*.flv", "*.gifv", "*.qt" } }
           });

        static FilePickerFileType PlatformPdfFileType() =>
            new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.WindowsDesktop, new[] { "*.pdf" } }
            });
    }
}
