using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace RedlabsUpdateUtility
{
	public class WindowIconTools
	{

		public static bool SetProgress(TaskbarProgressBarState state, ulong completed, ulong total)
		{
			var hwnd = mainWindow;
			if(hwnd == IntPtr.Zero)
				return false;
			var tbl = taskbarList;
			tbl.SetProgressState(hwnd, state);
			tbl.SetProgressValue(hwnd, completed, total);
			return true;
		}

		public static bool SetProgressState(TaskbarProgressBarState state)
		{
			var hwnd = mainWindow;
			if(hwnd == IntPtr.Zero)
				return false;
			taskbarList.SetProgressState(hwnd, state);
			return true;
		}

		public static bool SetProgressValue(ulong completed, ulong total)
		{
			var hwnd = mainWindow;
			if(hwnd == IntPtr.Zero)
				return false;
			taskbarList.SetProgressValue(hwnd, completed, total);
			return true;
		}

		private static object _initLock = new object();

		private static ITaskbarList4 _taskbarList;
		private static bool _taskbarListReady = false;
		private static ITaskbarList4 taskbarList
		{
			get
			{
				if(!_taskbarListReady)
				{
					lock(_initLock)
					{
						if(!_taskbarListReady)
						{
							try
							{
								_taskbarList = (ITaskbarList4)new CTaskbarList();
								_taskbarList.HrInit();
							}
							catch(Exception)
							{
								Console.WriteLine("ITaskbarList4 init failed!"
									+ " Go to Build Settings > Player Settings > Standalone > Other Settings,"
									+ " and set Api Compatibility Level to 4.x");
							}
							_taskbarListReady = true;
						}
					}
				}
				return _taskbarList;
			}
		}

		private static IntPtr _mainWindow = IntPtr.Zero;
		private static IntPtr mainWindow
		{
			get
			{
				if(_mainWindow == IntPtr.Zero)
				{
					lock(_initLock)
					{
						if(_mainWindow == IntPtr.Zero)
						{
							_mainWindow = GetActiveWindow();
						}
					}
				}
				return _mainWindow;
			}
		}

		[DllImport("user32.dll")]
		private static extern IntPtr GetActiveWindow();

		[ComImport, Guid("c43dc798-95d1-4bea-9030-bb99e2983a1a"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		private interface ITaskbarList4
		{
			[PreserveSig] void HrInit();
			[PreserveSig] void AddTab(IntPtr hwnd);
			[PreserveSig] void DeleteTab(IntPtr hwnd);
			[PreserveSig] void ActivateTab(IntPtr hwnd);
			[PreserveSig] void SetActiveAlt(IntPtr hwnd);
			[PreserveSig] void MarkFullscreenWindow(IntPtr hwnd, [MarshalAs(UnmanagedType.Bool)] bool fFullscreen);
			[PreserveSig] void SetProgressValue(IntPtr hwnd, ulong ullCompleted, ulong ullTotal);
			[PreserveSig] void SetProgressState(IntPtr hwnd, TaskbarProgressBarState tbpFlags);
			[PreserveSig] void RegisterTab(IntPtr hwndTab, IntPtr hwndMDI);
			[PreserveSig] void UnregisterTab(IntPtr hwndTab);
			[PreserveSig] void SetTabOrder(IntPtr hwndTab, IntPtr hwndInsertBefore);
			[PreserveSig] void SetTabActive(IntPtr hwndTab, IntPtr hwndInsertBefore, uint dwReserved);
			[PreserveSig] int ThumbBarAddButtons(IntPtr hwnd, uint cButtons, IntPtr pButtons);
			[PreserveSig] int ThumbBarUpdateButtons(IntPtr hwnd, uint cButtons, IntPtr pButtons);
			[PreserveSig] void ThumbBarSetImageList(IntPtr hwnd, IntPtr himl);
			[PreserveSig] void SetThumbnailTooltip(IntPtr hwnd, [MarshalAs(UnmanagedType.LPWStr)] string pszTip);
			[PreserveSig] void SetThumbnailClip(IntPtr hwnd, IntPtr prcClip);
		}
		[ComImport, Guid("56fdf344-fd6d-11d0-958a-006097c9a090"), ClassInterface(ClassInterfaceType.None)]
		private class CTaskbarList { }
	}

	public enum WindowIconKind
	{
		Small = 0,
		Big = 1
	}

	public enum TaskbarProgressBarState
	{
		NoProgress = 0,
		Indeterminate = 1,
		Normal = 2,
		Error = 4,
		Paused = 8
	}
}