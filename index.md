---
title: Agenda des communautés tech toulousaines
layout: default
---

<script type="text/javascript">
  function copyToClipboard(name) {
    var elt = document.getElementById(name);
    if(!elt) return;
    var url = elt.getAttribute('data-url');
    navigator.clipboard.writeText(url);
    
    var tooltip = bootstrap.Tooltip.getOrCreateInstance(elt, { title: 'URL copiée !', trigger: 'manual'});
    tooltip.show();
    setTimeout(function() { tooltip.hide()}, 3000);
  }
</script>

<!-- lead -->
<section class="py-1 text-center container">
  <div class="row">
    <div class="col-lg-6 col-md-8 mx-auto">
      <img src="logo.png" style="width: 100%" alt="Logo Toulouse Tech Hub" />
      <p class="lead text-muted">
          Retrouvez toutes les informations sur les communautés tech toulousaines sur une seule page.
      </p>
    </div>
  </div>
</section>

<!-- agenda -->
<section class="container" id="agenda">

  <!-- title -->
  <div class="row">
    <div class="mx-auto text-center">
      <h1 class="fw-light">Agenda</h1>
      <p class="lead text-muted">
          Tous les prochains évènements des communautés tech toulousaines.
      </p>
    </div>
  </div>

  {% include events.html %}

</section>

<!-- conférences -->
<section class="container my-2" id="conferences">

  <!-- title -->
  <div class="row">
    <div class="mx-auto text-center">
      <h1 class="fw-light">Conférences</h1>
      <p class="lead text-muted">
        Les grandes conférences tech annuelles sur Toulouse !
      </p>
    </div>
  </div>

  {% include conferences.html %}

</section>

<!-- communautés -->
<section class="container my-2" id="communautes">

  <!-- title -->
  <div class="row ">
    <div class="mx-auto text-center">
      <h1 class="fw-light">Communautés</h1>
      <p class="lead text-muted">
        La liste des communautés tech toulousaines référencées.
      </p>
    </div>
  </div>

  {% include groups.html %}

</section>

<footer class="text-muted py-5">
  <div class="container">
    <p class="float-end mb-1">
      <a href="#">⬆️Retour en haut</a>
    </p>
    <p>
      ❓Votre communauté ou votre évènement n'est pas présent et vous souhaitez qu'il soit ajouté ?<br>
      👉Venez le signaler sur le <a href="https://github.com/GDGToulouse/toulouse-tech-hub">projet github.com</a>.<br>
      ⚖️<a href="mentions.html">Mentions légales</a>
    </p>
  </div>
</footer>
