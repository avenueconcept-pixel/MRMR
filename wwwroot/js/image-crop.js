(function () {
  'use strict';

  var cropperInstance = null;
  var activeTrigger   = null;

  function initCropModal() {
    var modal     = document.getElementById('modalImageCrop');
    var imgSource = document.getElementById('imgCropSource');
    if (!modal || !imgSource) return;

    document.getElementById('modalCropTitle').textContent = modal.dataset.title   || 'Crop Image';
    document.getElementById('btnCropConfirm').textContent = modal.dataset.confirm || 'Crop & Use';
    document.getElementById('btnCropCancel').textContent  = modal.dataset.cancel  || 'Cancel';

    document.querySelectorAll('input[type="file"][data-crop="true"]').forEach(function (input) {
      input.addEventListener('change', function (e) {
        var file = e.target.files[0];
        if (!file) return;

        var ext = file.name.split('.').pop().toLowerCase();
        if (!['jpg', 'jpeg', 'png'].includes(ext)) {
          input.value = '';
          return;
        }

        activeTrigger = input;
        var reader = new FileReader();
        reader.onload = function (ev) {
          imgSource.src = ev.target.result;
          var bsModal = bootstrap.Modal.getOrCreateInstance(modal);
          bsModal.show();
          modal.addEventListener('shown.bs.modal', function onShown() {
            modal.removeEventListener('shown.bs.modal', onShown);
            if (cropperInstance) { cropperInstance.destroy(); cropperInstance = null; }
            var ratio      = input.dataset.cropRatio;
            var aspectRatio = (!ratio || ratio === 'free') ? NaN : parseFloat(ratio);
            cropperInstance = new Cropper(imgSource, {
              aspectRatio:      aspectRatio,
              viewMode:         1,
              autoCropArea:     0.8,
              responsive:       true,
              checkOrientation: true
            });
          });
        };
        reader.readAsDataURL(file);
      });
    });

    document.getElementById('btnCropConfirm').addEventListener('click', function () {
      if (!cropperInstance || !activeTrigger) return;
      var w   = parseInt(activeTrigger.dataset.cropOutputWidth  || 800);
      var h   = parseInt(activeTrigger.dataset.cropOutputHeight || 800);
      var ext = (activeTrigger.accept || '').toLowerCase().includes('png') ? 'png' : 'jpeg';
      cropperInstance.getCroppedCanvas({ width: w, height: h }).toBlob(function (blob) {
        var dt = new DataTransfer();
        dt.items.add(new File([blob], 'cropped.' + ext, { type: blob.type }));
        activeTrigger.files = dt.files;

        var previewSel = activeTrigger.dataset.cropPreview;
        if (previewSel) {
          var previewEl = document.querySelector(previewSel);
          if (previewEl) previewEl.src = URL.createObjectURL(blob);
        }

        bootstrap.Modal.getInstance(document.getElementById('modalImageCrop')).hide();
        cropperInstance.destroy();
        cropperInstance = null;
        activeTrigger   = null;
      }, 'image/' + ext);
    });

    document.getElementById('btnCropCancel').addEventListener('click', function () {
      if (activeTrigger) { activeTrigger.value = ''; activeTrigger = null; }
      if (cropperInstance) { cropperInstance.destroy(); cropperInstance = null; }
    });
  }

  document.addEventListener('DOMContentLoaded', initCropModal);
})();
