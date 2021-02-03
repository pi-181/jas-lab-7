using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace jaslab2
{
    public partial class MainForm : Form
    {
        private Assembly _assembly;

        public MainForm()
        {
            InitializeComponent();
        }

        private void OnOpenBuildClick(object sender, EventArgs e)
        {
            treeView1.Nodes.Clear();
            string path = selectAssemblyFile();
            if (path != null)
            {
                _assembly = openAssembly(path);
            }

            if (_assembly != null)
            {
                TreeNode root = new TreeNode {Text = _assembly.GetName().Name, ImageIndex = 0, SelectedImageIndex = 0};
                treeView1.Nodes.Add(root);
                Type[] types = _assembly.GetTypes();
                addRoot(root, types);
            }
        }

        private string selectAssemblyFile()
        {
            openFileDialog1.Filter = "Dll files (*.dll)|*.dll|Exe files (*.exe)|*.exe| All files (*.*)|*.*";
            openFileDialog1.Title = "Select assembly file";
            return (openFileDialog1.ShowDialog() == DialogResult.OK) ? openFileDialog1.FileName : null;
        }

        private Assembly openAssembly(string path)
        {
            try
            {
                Assembly a = Assembly.LoadFrom(path);
                return a;
            }
            catch (Exception)
            {
                MessageBox.Show("Не удалось загрузить указанную сборку!", 
                    "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        void addRoot(TreeNode root, Type[] types)
        {
            foreach (Type type in types)
            {
                if (type.IsClass) addRootElement(root, type, 1);
                else if (type.IsEnum) addRootElement(root, type, 2);
                else if (type.IsInterface) addRootElement(root, type, 3);
                else if (type.IsValueType) addRootElement(root, type, 10);
            }
        }

        private void addRootElement(TreeNode root, Type type, int icon)
        {
            var node = new TreeNode {Tag = type, Text = type.ToString(), ImageIndex = icon, SelectedImageIndex = icon};
            addFirstLevel(node, type);
            root.Nodes.Add(node);
        }

        private void addFirstLevel(TreeNode node, Type type)
        {
            TreeNode node1;

            var mods = ~BindingFlags.Default;
            FieldInfo[] fields = type.GetFields(mods);
            MethodInfo[] methods = type.GetMethods(mods);
            ConstructorInfo[] constructors = type.GetConstructors(mods);

            foreach (FieldInfo field in fields)
            {
                int icon = field.IsPrivate ? 4 : field.IsPublic ? 8 : 6; 
                node1 = new TreeNode
                {
                    Tag = field, Text = field.FieldType.Name + " " + field.Name, ImageIndex = icon, SelectedImageIndex = icon
                };
                node.Nodes.Add(node1);
            }

            foreach (ConstructorInfo constructor in constructors)
            {
                int icon = constructor.IsPrivate ? 11 : constructor.IsPublic ? 13 : 12; 
                String s = "";
                ParameterInfo[] paramsInfo = constructor.GetParameters();
                foreach (ParameterInfo param in paramsInfo)
                {
                    s = s + param.ParameterType.Name + ", ";
                }

                s = s.Trim();
                s = s.TrimEnd(',');
                node1 = new TreeNode {Tag = constructor, Text = node.Text + "(" + s + ")", ImageIndex = icon, SelectedImageIndex = icon};
                node.Nodes.Add(node1);
            }

            foreach (MethodInfo method in methods)
            {
                int icon = method.IsPrivate ? 5 : method.IsPublic ? 9 : 7; 
                
                String s = "";
                ParameterInfo[] paramsInfo = method.GetParameters();
                foreach (ParameterInfo param in paramsInfo)
                {
                    s = s + param.ParameterType.Name + ", ";
                }

                s = s.Trim();
                s = s.TrimEnd(',');
                node1 = new TreeNode
                {
                    Tag = method,
                    Text = method.ReturnType.Name + " " + method.Name + "(" + s + ")",
                    ImageIndex = icon,
                    SelectedImageIndex = icon
                };
                node.Nodes.Add(node1);
            }
        }

        private void OnItemClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            listView1.Clear();
            
            var tag = e.Node.Tag;
            if (tag == null)
            {
                listView1.Items.Add(new ListViewItem("No info about this node!"));
                return;
            }
            
            if (tag is ConstructorInfo cInfo) 
                DescribeConstructor(cInfo);
            else if (tag is MethodInfo mInfo) 
                DescribeMethod(mInfo);
            else if (tag is FieldInfo fInfo) 
                DescribeField(fInfo);
            else if (tag is Type type) 
                DescribeType(type);
            listView1.Update();
        }

        private void DescribeType(Type type)
        {
            var items = listView1.Items;
            items.Add("Назва: " + type.Name);
            items.Add(" ");
            
            items.Add("Модифікатори доступу: " + (
                          type.IsPublic ? "Публічний" : 
                          type.IsNestedPublic ? "Nested Public" : 
                          type.IsNestedPrivate ? "Nested Private" : "Приватний"));

            items.Add("Enum: " + (type.IsEnum ? "Так" : "Ні"));
            items.Add("Interface: " + (type.IsInterface ? "Так" : "Ні"));
            items.Add("Abstract: " + (type.IsAbstract ? "Так" : "Ні"));
            items.Add("Class: " + (type.IsClass ? "Так" : "Ні"));
            items.Add("Serializable: " + (type.IsSerializable ? "Так" : "Ні"));
            items.Add("Nested: " + (type.IsNested ? "Так" : "Ні"));
            items.Add("ValueType (structure): " + (type.IsValueType ? "Так" : "Ні"));
            items.Add("Transparent: " + (type.IsSecurityTransparent ? "Так" : "Ні"));
            items.Add("SecurityCritical: " + (type.IsSecurityCritical ? "Так" : "Ні"));
            items.Add("SecuritySafeCritical: " + (type.IsSecuritySafeCritical ? "Так" : "Ні"));
        }
        
        private void DescribeConstructor(ConstructorInfo info)
        {
            var items = listView1.Items;
            items.Add("Назва: " + info.Name);
            items.Add("Параметри: ");
            foreach (var pInfo in info.GetParameters())
            {
                items.Add( (pInfo.IsIn ? "In" : "Out") + " " + pInfo.ParameterType + " " + pInfo.Name);
            }
            items.Add(" ");
            
            items.Add("Constructor: " + (info.IsConstructor ? "Так" : "Ні"));
            items.Add("Видимість: " + (info.IsStatic ? "Статичне" : "Локальне"));
            items.Add("Модифікатори доступу: " 
                      + (info.IsPublic ? "Публічне" : info.IsPrivate ? "Приватне" : "Непублічне"));
            items.Add("Final: " + (info.IsFinal ? "Так" : "Ні"));
            items.Add("Abstract: " + (info.IsAbstract ? "Так" : "Ні"));
            items.Add("Assembly: " + (info.IsAssembly ? "Так" : "Ні"));
            items.Add("Transparent: " + (info.IsSecurityTransparent ? "Так" : "Ні"));
            items.Add("SecurityCritical: " + (info.IsSecurityCritical ? "Так" : "Ні"));
            items.Add("SecuritySafeCritical: " + (info.IsSecuritySafeCritical ? "Так" : "Ні"));
        }
        
        private void DescribeMethod(MethodInfo info)
        {
            var items = listView1.Items;
            items.Add("Назва: " + info.Name);
            items.Add("Повертаємий тип: " + info.ReturnType);
            items.Add("Параметри: ");
            foreach (var pInfo in info.GetParameters())
            {
                items.Add( (pInfo.IsIn ? "In" : "Out") + " " + pInfo.ParameterType + " " + pInfo.Name);
            }
            items.Add(" ");
            
            items.Add("Constructor: " + (info.IsConstructor ? "Так" : "Ні"));
            items.Add("Видимість: " + (info.IsStatic ? "Статичне" : "Локальне"));
            items.Add("Модифікатори доступу: " 
                      + (info.IsPublic ? "Публічне" : info.IsPrivate ? "Приватне" : "Непублічне"));
            items.Add("Final: " + (info.IsFinal ? "Так" : "Ні"));
            items.Add("Abstract: " + (info.IsAbstract ? "Так" : "Ні"));
            items.Add("Assembly: " + (info.IsAssembly ? "Так" : "Ні"));
            items.Add("Transparent: " + (info.IsSecurityTransparent ? "Так" : "Ні"));
            items.Add("SecurityCritical: " + (info.IsSecurityCritical ? "Так" : "Ні"));
            items.Add("SecuritySafeCritical: " + (info.IsSecuritySafeCritical ? "Так" : "Ні"));
        }
        
        private void DescribeField(FieldInfo info)
        {
            var items = listView1.Items;
            items.Add("Назва: " + info.Name);
            items.Add("Тип: " + info.FieldType);
            items.Add(" ");

            items.Add("Видимість: " + (info.IsStatic ? "Статичне" : "Локальне"));
            items.Add("Модифікатори доступу: " 
                      + (info.IsPublic ? "Публічне" : info.IsPrivate ? "Приватне" : "Непублічне"));
            items.Add("Init: " + (info.IsInitOnly ? "Так" : "Ні"));
            items.Add("Assembly: " + (info.IsAssembly ? "Так" : "Ні"));
            items.Add("Transparent: " + (info.IsSecurityTransparent ? "Так" : "Ні"));
            items.Add("Literal: " + (info.IsLiteral ? "Так" : "Ні"));
            items.Add("SecurityCritical: " + (info.IsSecurityCritical ? "Так" : "Ні"));
            items.Add("SecuritySafeCritical: " + (info.IsSecuritySafeCritical ? "Так" : "Ні"));
        }
        
    }
}