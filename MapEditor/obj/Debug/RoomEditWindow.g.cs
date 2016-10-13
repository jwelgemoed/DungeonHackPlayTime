﻿#pragma checksum "..\..\RoomEditWindow.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "F1D3A0B1A7A8E7FD0C355046AF3FC51D"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace MapEditor {
    
    
    /// <summary>
    /// RoomEditWindow
    /// </summary>
    public partial class RoomEditWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 7 "..\..\RoomEditWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Canvas canvasXZ;
        
        #line default
        #line hidden
        
        
        #line 8 "..\..\RoomEditWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnSave;
        
        #line default
        #line hidden
        
        
        #line 9 "..\..\RoomEditWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox txtRoomName;
        
        #line default
        #line hidden
        
        
        #line 10 "..\..\RoomEditWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label lblName;
        
        #line default
        #line hidden
        
        
        #line 11 "..\..\RoomEditWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image imgTexture;
        
        #line default
        #line hidden
        
        
        #line 12 "..\..\RoomEditWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListBox lstTextures;
        
        #line default
        #line hidden
        
        
        #line 13 "..\..\RoomEditWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label lblPreview;
        
        #line default
        #line hidden
        
        
        #line 14 "..\..\RoomEditWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label lblSelected;
        
        #line default
        #line hidden
        
        
        #line 15 "..\..\RoomEditWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnLineSegment;
        
        #line default
        #line hidden
        
        
        #line 16 "..\..\RoomEditWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnSector;
        
        #line default
        #line hidden
        
        
        #line 17 "..\..\RoomEditWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox chkGrid;
        
        #line default
        #line hidden
        
        
        #line 18 "..\..\RoomEditWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox chkSnapToGrid;
        
        #line default
        #line hidden
        
        
        #line 19 "..\..\RoomEditWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox chkSnapToClosestPoint;
        
        #line default
        #line hidden
        
        
        #line 20 "..\..\RoomEditWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox chkSolidSegment;
        
        #line default
        #line hidden
        
        
        #line 21 "..\..\RoomEditWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnSetSegmentTexture;
        
        #line default
        #line hidden
        
        
        #line 22 "..\..\RoomEditWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnTriangulate;
        
        #line default
        #line hidden
        
        
        #line 23 "..\..\RoomEditWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.GroupBox grpSector;
        
        #line default
        #line hidden
        
        
        #line 25 "..\..\RoomEditWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label lblFloorHeight;
        
        #line default
        #line hidden
        
        
        #line 26 "..\..\RoomEditWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox txtFloorHeight;
        
        #line default
        #line hidden
        
        
        #line 27 "..\..\RoomEditWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label lblCeilingHeight;
        
        #line default
        #line hidden
        
        
        #line 28 "..\..\RoomEditWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox txtCeilingHeight;
        
        #line default
        #line hidden
        
        
        #line 29 "..\..\RoomEditWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnSetFloorTexture;
        
        #line default
        #line hidden
        
        
        #line 30 "..\..\RoomEditWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnSetCeilingTexture;
        
        #line default
        #line hidden
        
        
        #line 32 "..\..\RoomEditWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image imgFloor;
        
        #line default
        #line hidden
        
        
        #line 33 "..\..\RoomEditWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label lblFloor;
        
        #line default
        #line hidden
        
        
        #line 34 "..\..\RoomEditWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image imgCeiling;
        
        #line default
        #line hidden
        
        
        #line 35 "..\..\RoomEditWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label lblCeiling;
        
        #line default
        #line hidden
        
        
        #line 36 "..\..\RoomEditWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnSaveSector;
        
        #line default
        #line hidden
        
        
        #line 37 "..\..\RoomEditWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox txtId;
        
        #line default
        #line hidden
        
        
        #line 38 "..\..\RoomEditWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label lblId;
        
        #line default
        #line hidden
        
        
        #line 41 "..\..\RoomEditWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image imgTextureLineSeg;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/MapEditor;component/roomeditwindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\RoomEditWindow.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.canvasXZ = ((System.Windows.Controls.Canvas)(target));
            
            #line 7 "..\..\RoomEditWindow.xaml"
            this.canvasXZ.Loaded += new System.Windows.RoutedEventHandler(this.Canvas_Loaded);
            
            #line default
            #line hidden
            
            #line 7 "..\..\RoomEditWindow.xaml"
            this.canvasXZ.MouseMove += new System.Windows.Input.MouseEventHandler(this.Canvas_MouseMove);
            
            #line default
            #line hidden
            
            #line 7 "..\..\RoomEditWindow.xaml"
            this.canvasXZ.PreviewMouseRightButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.canvasXZ_PreviewMouseRightButtonDown);
            
            #line default
            #line hidden
            
            #line 7 "..\..\RoomEditWindow.xaml"
            this.canvasXZ.PreviewMouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.canvasXZ_PreviewMouseLeftButtonDown);
            
            #line default
            #line hidden
            
            #line 7 "..\..\RoomEditWindow.xaml"
            this.canvasXZ.PreviewKeyDown += new System.Windows.Input.KeyEventHandler(this.canvasXZ_PreviewKeyDown);
            
            #line default
            #line hidden
            return;
            case 2:
            this.btnSave = ((System.Windows.Controls.Button)(target));
            
            #line 8 "..\..\RoomEditWindow.xaml"
            this.btnSave.Click += new System.Windows.RoutedEventHandler(this.btnSave_Click);
            
            #line default
            #line hidden
            return;
            case 3:
            this.txtRoomName = ((System.Windows.Controls.TextBox)(target));
            return;
            case 4:
            this.lblName = ((System.Windows.Controls.Label)(target));
            return;
            case 5:
            this.imgTexture = ((System.Windows.Controls.Image)(target));
            return;
            case 6:
            this.lstTextures = ((System.Windows.Controls.ListBox)(target));
            
            #line 12 "..\..\RoomEditWindow.xaml"
            this.lstTextures.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.lstTextures_SelectionChanged);
            
            #line default
            #line hidden
            
            #line 12 "..\..\RoomEditWindow.xaml"
            this.lstTextures.MouseDoubleClick += new System.Windows.Input.MouseButtonEventHandler(this.lstTextures_MouseDoubleClick);
            
            #line default
            #line hidden
            return;
            case 7:
            this.lblPreview = ((System.Windows.Controls.Label)(target));
            return;
            case 8:
            this.lblSelected = ((System.Windows.Controls.Label)(target));
            return;
            case 9:
            this.btnLineSegment = ((System.Windows.Controls.Button)(target));
            
            #line 15 "..\..\RoomEditWindow.xaml"
            this.btnLineSegment.Click += new System.Windows.RoutedEventHandler(this.btnLineSegment_Click);
            
            #line default
            #line hidden
            return;
            case 10:
            this.btnSector = ((System.Windows.Controls.Button)(target));
            
            #line 16 "..\..\RoomEditWindow.xaml"
            this.btnSector.Click += new System.Windows.RoutedEventHandler(this.btnSector_Click);
            
            #line default
            #line hidden
            return;
            case 11:
            this.chkGrid = ((System.Windows.Controls.CheckBox)(target));
            
            #line 17 "..\..\RoomEditWindow.xaml"
            this.chkGrid.Click += new System.Windows.RoutedEventHandler(this.chkGrid_Click);
            
            #line default
            #line hidden
            return;
            case 12:
            this.chkSnapToGrid = ((System.Windows.Controls.CheckBox)(target));
            
            #line 18 "..\..\RoomEditWindow.xaml"
            this.chkSnapToGrid.Click += new System.Windows.RoutedEventHandler(this.chkSnapToGrid_Click);
            
            #line default
            #line hidden
            return;
            case 13:
            this.chkSnapToClosestPoint = ((System.Windows.Controls.CheckBox)(target));
            
            #line 19 "..\..\RoomEditWindow.xaml"
            this.chkSnapToClosestPoint.Click += new System.Windows.RoutedEventHandler(this.chkSnapToClosestPoint_Click);
            
            #line default
            #line hidden
            return;
            case 14:
            this.chkSolidSegment = ((System.Windows.Controls.CheckBox)(target));
            return;
            case 15:
            this.btnSetSegmentTexture = ((System.Windows.Controls.Button)(target));
            
            #line 21 "..\..\RoomEditWindow.xaml"
            this.btnSetSegmentTexture.Click += new System.Windows.RoutedEventHandler(this.btnSetSegmentTexture_Click);
            
            #line default
            #line hidden
            return;
            case 16:
            this.btnTriangulate = ((System.Windows.Controls.Button)(target));
            
            #line 22 "..\..\RoomEditWindow.xaml"
            this.btnTriangulate.Click += new System.Windows.RoutedEventHandler(this.btnTriangulate_Click);
            
            #line default
            #line hidden
            return;
            case 17:
            this.grpSector = ((System.Windows.Controls.GroupBox)(target));
            return;
            case 18:
            this.lblFloorHeight = ((System.Windows.Controls.Label)(target));
            return;
            case 19:
            this.txtFloorHeight = ((System.Windows.Controls.TextBox)(target));
            return;
            case 20:
            this.lblCeilingHeight = ((System.Windows.Controls.Label)(target));
            return;
            case 21:
            this.txtCeilingHeight = ((System.Windows.Controls.TextBox)(target));
            return;
            case 22:
            this.btnSetFloorTexture = ((System.Windows.Controls.Button)(target));
            
            #line 29 "..\..\RoomEditWindow.xaml"
            this.btnSetFloorTexture.Click += new System.Windows.RoutedEventHandler(this.btnSetFloorTexture_Click);
            
            #line default
            #line hidden
            return;
            case 23:
            this.btnSetCeilingTexture = ((System.Windows.Controls.Button)(target));
            
            #line 30 "..\..\RoomEditWindow.xaml"
            this.btnSetCeilingTexture.Click += new System.Windows.RoutedEventHandler(this.btnSetCeilingTexture_Click);
            
            #line default
            #line hidden
            return;
            case 24:
            this.imgFloor = ((System.Windows.Controls.Image)(target));
            return;
            case 25:
            this.lblFloor = ((System.Windows.Controls.Label)(target));
            return;
            case 26:
            this.imgCeiling = ((System.Windows.Controls.Image)(target));
            return;
            case 27:
            this.lblCeiling = ((System.Windows.Controls.Label)(target));
            return;
            case 28:
            this.btnSaveSector = ((System.Windows.Controls.Button)(target));
            return;
            case 29:
            this.txtId = ((System.Windows.Controls.TextBox)(target));
            return;
            case 30:
            this.lblId = ((System.Windows.Controls.Label)(target));
            return;
            case 31:
            this.imgTextureLineSeg = ((System.Windows.Controls.Image)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

