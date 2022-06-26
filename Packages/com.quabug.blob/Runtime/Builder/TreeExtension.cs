using System;
using System.Collections.Generic;
using System.Linq;

namespace Blob
{
    public static class TreeExtension
    {
        public static IEnumerable<TTreeNode> SelfAndDescendants<TTreeNode>(this TTreeNode root, Func<TTreeNode, IEnumerable<TTreeNode>> getChildren)
        {
            return root.Yield().Concat(getChildren(root).SelectMany(child => SelfAndDescendants(child, getChildren)));
        }

        private static IEnumerable<T> Yield<T>(this T value)
        {
            yield return value;
        }
    }
}