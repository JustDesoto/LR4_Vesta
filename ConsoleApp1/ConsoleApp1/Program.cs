using System;
using System.Collections.Generic;

class BTreeNode
{
    public List<int> Keys { get; set; }
    public List<BTreeNode> Children { get; set; }
    public bool IsLeaf => Children.Count == 0;

    public BTreeNode(int t)
    {
        Keys = new List<int>();
        Children = new List<BTreeNode>();
    }
}

class BTree
{
    private int T; // Порядок дерева (половина порядка)
    private BTreeNode Root;

    public BTree(int t)
    {
        T = t;
        Root = new BTreeNode(T);
    }

    // Поиск ключа в B-дереве
    public bool Search(int key)
    {
        return Search(Root, key);
    }

    private bool Search(BTreeNode node, int key)
    {
        int i = 0;
        while (i < node.Keys.Count && key > node.Keys[i])
            i++;

        if (i < node.Keys.Count && key == node.Keys[i])
            return true; // Ключ найден

        if (node.IsLeaf)
            return false; // Ключ не найден

        return Search(node.Children[i], key); // Переход к соответствующему дочернему узлу
    }

    // Вставка ключа в B-дерево
    public void Insert(int key)
    {
        BTreeNode root = Root;
        if (root.Keys.Count == 2 * T - 1)
        {
            // Если корень переполнен, создаем новый корень
            BTreeNode newRoot = new BTreeNode(T);
            newRoot.Children.Add(Root);
            SplitChild(newRoot, 0);
            Root = newRoot;
        }
        InsertNonFull(Root, key);
    }

    private void InsertNonFull(BTreeNode node, int key)
    {
        int i = node.Keys.Count - 1;
        if (node.IsLeaf)
        {
            // Вставляем ключ в листовой узел
            node.Keys.Add(0);
            while (i >= 0 && key < node.Keys[i])
            {
                node.Keys[i + 1] = node.Keys[i];
                i--;
            }
            node.Keys[i + 1] = key;
        }
        else
        {
            // Находим подходящий дочерний узел
            while (i >= 0 && key < node.Keys[i])
                i--;
            i++;

            if (node.Children[i].Keys.Count == 2 * T - 1)
            {
                SplitChild(node, i);
                if (key > node.Keys[i])
                    i++;
            }
            InsertNonFull(node.Children[i], key);
        }
    }

    // Разделение переполненного дочернего узла
    private void SplitChild(BTreeNode parent, int index)
    {
        BTreeNode fullChild = parent.Children[index];
        BTreeNode newChild = new BTreeNode(T);

        parent.Keys.Insert(index, fullChild.Keys[T - 1]);
        parent.Children.Insert(index + 1, newChild);

        newChild.Keys.AddRange(fullChild.Keys.GetRange(T, T - 1));
        fullChild.Keys.RemoveRange(T - 1, T);

        if (!fullChild.IsLeaf)
        {
            newChild.Children.AddRange(fullChild.Children.GetRange(T, T));
            fullChild.Children.RemoveRange(T, T);
        }
    }

    // Удаление ключа из B-дерева
    public void Delete(int key)
    {
        Delete(Root, key);
        if (Root.Keys.Count == 0 && !Root.IsLeaf)
        {
            Root = Root.Children[0];
        }
    }

    private void Delete(BTreeNode node, int key)
    {
        int i = 0;
        while (i < node.Keys.Count && key > node.Keys[i])
            i++;

        if (i < node.Keys.Count && key == node.Keys[i])
        {
            if (node.IsLeaf)
            {
                node.Keys.RemoveAt(i);
            }
            else
            {
                DeleteInternal(node, i);
            }
        }
        else if (!node.IsLeaf)
        {
            Delete(node.Children[i], key);
        }
    }

    private void DeleteInternal(BTreeNode node, int index)
    {
        int key = node.Keys[index];

        if (node.Children[index].Keys.Count >= T)
        {
            BTreeNode pred = node.Children[index];
            while (!pred.IsLeaf)
                pred = pred.Children[pred.Children.Count - 1];
            node.Keys[index] = pred.Keys[pred.Keys.Count - 1];
            Delete(pred, pred.Keys[pred.Keys.Count - 1]);
        }
        else if (node.Children[index + 1].Keys.Count >= T)
        {
            BTreeNode succ = node.Children[index + 1];
            while (!succ.IsLeaf)
                succ = succ.Children[0];
            node.Keys[index] = succ.Keys[0];
            Delete(succ, succ.Keys[0]);
        }
        else
        {
            MergeChildren(node, index);
            Delete(node.Children[index], key);
        }
    }

    private void MergeChildren(BTreeNode parent, int index)
    {
        BTreeNode leftChild = parent.Children[index];
        BTreeNode rightChild = parent.Children[index + 1];

        leftChild.Keys.Add(parent.Keys[index]);
        leftChild.Keys.AddRange(rightChild.Keys);
        leftChild.Children.AddRange(rightChild.Children);

        parent.Keys.RemoveAt(index);
        parent.Children.RemoveAt(index + 1);
    }

    // Печать дерева
    public void Print()
    {
        Print(Root, "", true);
    }

    private void Print(BTreeNode node, string indent, bool last)
    {
        Console.WriteLine(indent + "+- " + (last ? "R" : "L") + ": " + string.Join(", ", node.Keys));
        indent += last ? "   " : "|  ";

        for (int i = 0; i < node.Children.Count; i++)
            Print(node.Children[i], indent, i == node.Children.Count - 1);
    }
}

class Program
{
    static void Main()
    {
        BTree tree = new BTree(3); // B-дерево порядка 5 (T=3)
        Random random = new Random();

        // Ввод данных
        Console.WriteLine("Введите 10-15 целых чисел (разделяйте пробелами):");
        string input = Console.ReadLine();
        string[] parts = input.Split();
        foreach (string part in parts)
        {
            if (int.TryParse(part, out int number))
                tree.Insert(number);
        }

        // Вывод дерева
        Console.WriteLine("\nB-дерево после вставки элементов:");
        tree.Print();

        // Поиск элемента
        Console.WriteLine("\nВведите число для поиска:");
        int searchKey = int.Parse(Console.ReadLine());
        bool found = tree.Search(searchKey);
        Console.WriteLine(found ? "Число найдено!" : "Число не найдено.");

        // Удаление элемента
        Console.WriteLine("\nВведите число для удаления:");
        int deleteKey = int.Parse(Console.ReadLine());
        tree.Delete(deleteKey);

        // Вывод дерева после удаления
        Console.WriteLine("\nB-дерево после удаления элемента:");
        tree.Print();
    }
}
