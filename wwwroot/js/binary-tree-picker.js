/**
 * Binary Tree Picker
 * Usage:
 *   BinaryTreePicker.open({
 *     rootId:   sponsorId,
 *     onSelect: function(parentId, position, parentUsername) { ... }
 *   });
 */
var BinaryTreePicker = (function () {

  var _rootId     = null;
  var _onSelect   = null;
  var _breadcrumb = [];

  function open(options) {
    _rootId     = options.rootId;
    _onSelect   = options.onSelect;
    _breadcrumb = [];

    var modal = new bootstrap.Modal(document.getElementById('modalBinaryTree'));
    modal.show();
    loadNode(_rootId, true);
  }

  function loadNode(nodeId, resetBreadcrumb) {
    showLoading(true);

    if (resetBreadcrumb) {
      _breadcrumb = [];
    }

    fetch('/Admin/Members?handler=BinaryNode&rootId=' + nodeId, {
      headers: { 'X-Requested-With': 'XMLHttpRequest' }
    })
    .then(function (r) { return r.json(); })
    .then(function (data) {
      showLoading(false);
      if (!data.success) {
        document.getElementById('binaryTreeContainer').innerHTML =
          '<p class="text-danger text-center">Failed to load tree.</p>';
        return;
      }
      renderBreadcrumb();
      renderTree(data.node);
    })
    .catch(function () {
      showLoading(false);
      document.getElementById('binaryTreeContainer').innerHTML =
        '<p class="text-danger text-center">Error loading tree.</p>';
    });
  }

  function renderBreadcrumb() {
    var list = document.getElementById('binaryBreadcrumbList');
    list.innerHTML = '';
    _breadcrumb.forEach(function (crumb, index) {
      var li = document.createElement('li');
      if (index === _breadcrumb.length - 1) {
        li.className = 'breadcrumb-item active';
        li.textContent = crumb.username;
      } else {
        li.className = 'breadcrumb-item';
        var a = document.createElement('a');
        a.href = '#';
        a.textContent = crumb.username;
        a.dataset.index = index;
        a.addEventListener('click', function (e) {
          e.preventDefault();
          var idx = parseInt(this.dataset.index);
          var targetId = _breadcrumb[idx].id;
          _breadcrumb = _breadcrumb.slice(0, idx);
          loadNode(targetId, false);
        });
        li.appendChild(a);
      }
      list.appendChild(li);
    });
  }

  function renderTree(node) {
    var container = document.getElementById('binaryTreeContainer');
    container.innerHTML = '';

    var wrapper = document.createElement('div');
    wrapper.className = 'binary-tree';
    wrapper.appendChild(renderSubtree(node));
    container.appendChild(wrapper);
  }

  function renderSubtree(node) {
    var subtree = document.createElement('div');
    subtree.className = 'binary-subtree';

    var box = document.createElement('div');

    if (node.__isEmpty) {
      box.className = 'binary-node binary-node-empty';
      box.innerHTML = '<i class="ri ri-add-circle-line fs-4"></i><br><small>Place here</small>';
      box.dataset.parentId       = node.__parentId;
      box.dataset.position       = node.__position;
      box.dataset.parentUsername = node.__parentUsername || '';
      box.addEventListener('click', function () {
        if (_onSelect) {
          _onSelect(
            parseInt(this.dataset.parentId),
            this.dataset.position,
            this.dataset.parentUsername
          );
        }
        bootstrap.Modal.getInstance(document.getElementById('modalBinaryTree')).hide();
      });
    } else if (node.hasMoreBelow) {
      box.className = 'binary-node binary-node-filled binary-node-deeper';
      box.innerHTML = buildNodeHTML(node) +
        '<div class="binary-deeper-hint"><i class="ri ri-arrow-down-line"></i> more</div>';
      box.dataset.id = node.id;
      box.style.cursor = 'pointer';
      box.addEventListener('click', function () {
        var nodeId       = parseInt(this.dataset.id);
        var nodeUsername = this.querySelector('.binary-username').textContent;
        _breadcrumb.push({ id: nodeId, username: nodeUsername });
        loadNode(nodeId, false);
      });
    } else {
      box.className = 'binary-node binary-node-filled';
      box.innerHTML = buildNodeHTML(node);
      box.dataset.id = node.id;

      var leftNode  = node.left  || { __isEmpty: true, __parentId: node.id, __position: 'left',  __parentUsername: node.username };
      var rightNode = node.right || { __isEmpty: true, __parentId: node.id, __position: 'right', __parentUsername: node.username };

      var hasChildren = node.left || node.right;
      if (hasChildren) {
        box.style.cursor = 'pointer';
        box.title = 'Click to zoom in';
        box.addEventListener('click', function () {
          var nodeId       = parseInt(this.dataset.id);
          var nodeUsername = this.querySelector('.binary-username').textContent;
          _breadcrumb.push({ id: nodeId, username: nodeUsername });
          loadNode(nodeId, false);
        });
      }

      var childrenDiv = document.createElement('div');
      childrenDiv.className = 'binary-children';
      childrenDiv.appendChild(renderSubtree(leftNode));
      childrenDiv.appendChild(renderSubtree(rightNode));
      subtree.appendChild(box);
      subtree.appendChild(childrenDiv);
      return subtree;
    }

    subtree.appendChild(box);
    return subtree;
  }

  function buildNodeHTML(node) {
    var activatedBadge = node.isActivated
      ? '<span class="badge bg-success ms-1">Active</span>'
      : '<span class="badge bg-secondary ms-1">Free</span>';
    var rankBadge = node.rankCode
      ? '<span class="badge bg-info ms-1">' + escHtml(node.rankCode) + '</span>'
      : '';
    return '<div class="binary-username fw-semibold">' + escHtml(node.username) + '</div>' +
           '<div class="binary-fullname small text-muted">'  + escHtml(node.fullName)  + '</div>' +
           '<div class="binary-badges mt-1">' + activatedBadge + rankBadge + '</div>';
  }

  function escHtml(str) {
    var d = document.createElement('div');
    d.appendChild(document.createTextNode(str || ''));
    return d.innerHTML;
  }

  function showLoading(show) {
    document.getElementById('binaryTreeLoading').classList.toggle('d-none', !show);
    document.getElementById('binaryTreeContainer').classList.toggle('d-none', show);
  }

  return { open: open };

})();
