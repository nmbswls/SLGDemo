using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogicTreeChecker
{
	public virtual bool CheckNode(string paramstring)
	{
		return true;
	}

	public bool CheckTree(LogicTreeNode root)
	{
		bool ret;
		if (root.type == eLogicType.Leaf)
		{
			ret = CheckNode(root.checkstring);
		}
		else if (root.type == eLogicType.AND)
		{
			ret = true;
			foreach (LogicTreeNode child in root.ChildNodes)
			{
				ret &= CheckTree(child);
				if (!ret)
					break;
			}
		}
		else
		{
			ret = false;
			foreach (LogicTreeNode child in root.ChildNodes)
			{
				ret |= CheckTree(child);
				if (ret)
					break;
			}
		}
		return root.anti ? !ret : ret;
	}
}
public class LogicTree
{
    public LogicTreeNode Header;

	public static LogicTreeNode ConstructFromString(string input)
	{
		string str = input.Trim();
		Stack<LogicTreeNode> StackNode = new Stack<LogicTreeNode>();
		Stack<char> StackOpt = new Stack<char>();

		Dictionary<char, int> optPriority = new Dictionary<char, int>();
		optPriority['('] = 0;
		optPriority['|'] = 1;
		optPriority['&'] = 2;
		optPriority[')'] = 3;

		for (int i = 0; i < str.Length; i++)
		{
			if (char.IsWhiteSpace(input[i]))
			{
				continue;
			}
			if (str[i] == '!' || char.IsLetterOrDigit(str[i]))
			{
				bool anti = false;
				if (str[i] == '!')
				{
					i += 1;
					anti = true;
				}

				string checkstring;
				int j = i;
				while (j < str.Length && char.IsLetterOrDigit(str[j]))
				{
					j++;
				}
				if (j >= str.Length)
				{
					checkstring = str.Substring(i, j-i);
					i = j - 1;
				}
				else
				{
					if (j < str.Length && str[j] == '(')
					{
						while (j < str.Length && str[j] != ')')
						{
							j++;
						}
						if (j >= str.Length)
						{
							Debug.Log("Error Invalid No Right Kuohao");
							throw new UnityException("error invalid");
						}
						checkstring = str.Substring(i, j - i);
						i = j;
					}
					else
					{
						checkstring = str.Substring(i, j - i);
						i = j - 1;
					}
				}				
				LogicTreeNode node = new LogicTreeNode(eLogicType.Leaf);
				node.anti = anti;
				node.checkstring = checkstring;
				StackNode.Push(node);
			}
			else
			{
				if (StackOpt.Count == 0)
				{
					StackOpt.Push(str[i]);
					continue;
				}
				if (str[i] == ')')
				{
					while (StackOpt.Count > 0 && StackOpt.Peek() != '(')
					{
						LogicTreeNode n2 = StackNode.Pop();
						LogicTreeNode n1 = StackNode.Pop();
						char op = StackOpt.Pop();
						LogicTreeNode newNode = MergeNode(n1, n2, op);
						StackNode.Push(newNode);
					}

					if (StackOpt.Peek() == '(') 
						StackOpt.Pop();
				}
				else
				{

					if (str[i] != '(' && optPriority[str[i]] > optPriority[StackOpt.Peek()])
					{
						StackOpt.Push(str[i]);
						continue;
					}

					while (StackOpt.Count > 0 && str[i] != '(' && optPriority[str[i]] <= optPriority[StackOpt.Peek()])
					{
						LogicTreeNode n2 = StackNode.Pop();
						LogicTreeNode n1 = StackNode.Pop();
						char op = StackOpt.Pop();
						LogicTreeNode newNode = MergeNode(n1, n2, op);
						StackNode.Push(newNode);
					}

					StackOpt.Push(str[i]);
				}
			}
		}

		while (StackOpt.Count > 0)
		{
			LogicTreeNode n2 = StackNode.Pop();
			LogicTreeNode n1 = StackNode.Pop();
			char op = StackOpt.Pop();
			LogicTreeNode newNode = MergeNode(n1, n2, op);
			StackNode.Push(newNode);
		}

		return StackNode.Pop();
	}


	private static LogicTreeNode MergeNode(LogicTreeNode n1, LogicTreeNode n2, char opt)
	{
		LogicTreeNode ret = null;
		if (opt == '&')
		{
			if(n1.type == eLogicType.AND)
			{
				n1.ApeendChild(n2);
				ret = n1;
			}
			else if (n2.type == eLogicType.AND)
			{
				n2.ApeendChild(n1);
				ret = n2;
			}
			else
			{
				ret = new LogicTreeNode(eLogicType.AND);
				ret.ApeendChild(n1);
				ret.ApeendChild(n2);
			}
			
		}
		else if (opt == '|')
		{
			if (n1.type == eLogicType.OR)
			{
				n1.ApeendChild(n2);
				ret = n1;
			}
			else if (n2.type == eLogicType.OR)
			{
				n2.ApeendChild(n1);
				ret = n2;
			}
			else
			{
				ret = new LogicTreeNode(eLogicType.OR);
				ret.ApeendChild(n1);
				ret.ApeendChild(n2);
			}
		}
		else
		{
			Debug.Log("Wrong Opt");
			return null;
		}
		
		return ret;
	}



}

public enum eLogicType
{
	Leaf,
	AND,
	OR
}

public class LogicTreeNode
{
	public string checkstring;
	public eLogicType type = eLogicType.Leaf;
	public bool anti = false;
	public List<LogicTreeNode> ChildNodes = new List<LogicTreeNode>();

	public LogicTreeNode(eLogicType type)
	{
		this.type = type;
	}

	protected virtual bool CheckLeaf()
	{
		return true;
	}

	

	public void ApeendChild(LogicTreeNode child)
	{
		ChildNodes.Add(child);
	}
}

