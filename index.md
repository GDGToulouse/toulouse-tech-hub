---
title: Agenda des communautés tech toulousaines
layout: default
---

<script type="text/javascript">
  function copyToClipboard(name) {
    var elt = document.getElementById(name);
    if(!elt) return;
    var url = elt.getAttribute('data-url');
    /*
    elt.select();
    elt.setSelectionRange(0, 99999); // for mobile devices??
    */
    navigator.clipboard.writeText(url);
    
    var tooltip = bootstrap.Tooltip.getOrCreateInstance(elt, { title: 'URL copiée !', trigger: 'manual'});
    tooltip.show();
    setTimeout(function() { tooltip.hide()}, 3000);
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
<div class="container">

  <!-- title -->
  <div class="row">
    <div class="mx-auto text-center">
      <h1 class="fw-light">Agenda</h1>
      <p class="lead text-muted">
          Tous les prochains évènements des communautés tech toulousaines.
      </p>
    </div>
  </div>

  <!-- ics / atom -->
  <div class="row justify-content-center">
    <div class="my-2 col-lg-4 col-md-8">
      <div class="input-group">
        <span class="input-group-text"><i class="bi bi-calendar2-week"></i>&nbsp;iCal</span>
        <input id="icsInput" type="text" value="{{ site.site }}{{ site.baseurl }}/events.ics" data-url="{{ site.site }}{{ site.baseurl }}/events.ics" readonly class="form-control" style="background-color:#fff" onfocus="this.select()">
        <button class="btn btn-outline-secondary" type="button" id="button-addon-ics" title="Copier l'URL dans le presse-papier" onclick="copyToClipboard('icsInput')"><i class="bi bi-clipboard-check"></i></button>
      </div>
    </div>
    <div class="my-2 col-lg-4 col-md-8">
      <div class="input-group">
        <span class="input-group-text"><i class="bi bi-rss"></i>&nbsp;Rss/Atom</span>
        <input id="atomInput" type="text" value="{{ site.site }}{{ site.baseurl }}/events.atom.xml" data-url="{{ site.site }}{{ site.baseurl }}/events.atom.xml" readonly class="form-control" style="background-color:#fff" onfocus="this.select()">
        <button class="btn btn-outline-secondary" type="button" id="button-addon-atom" title="Copier l'URL dans le presse-papier" onclick="copyToClipboard('atomInput')"><i class="bi bi-clipboard-check"></i></button>
      </div>
    </div>
  </div>

  <!-- json -->
  <div class="row justify-content-center">
    <div class="my-2 col-lg-4 col-md-8">
      <div class="input-group">
        <span class="input-group-text"><i class="bi bi-braces"></i>&nbsp;Json</span>
        <input id="jsonInput" type="text" value="{{ site.site }}{{ site.baseurl }}/events.json" data-url="{{ site.site }}{{ site.baseurl }}/events.json" readonly class="form-control" style="background-color:#fff" onfocus="this.select()">
        <button class="btn btn-outline-secondary" type="button" id="button-addon-json" title="Copier l'URL dans le presse-papier" onclick="copyToClipboard('jsonInput')"><i class="bi bi-clipboard-check"></i></button>
      </div>
    </div>
  </div>

  <!-- grid -->
  <div class="row row-cols-1 row-cols-md-2 row-cols-xxl-3 g-3 agenda my-2">

{%- assign now_time = site.time | date: "%s" | plus: 0 -%}
{%- for event_hash in site.data.events -%}
{%- assign event = event_hash[1] -%}
{%- assign event_time = event.dateIso | date:"%s" | plus: 0 -%}
{%- if event_time < now_time -%}
  {%- continue -%}
{%- endif -%}

  <div class="col">
    <div class="card shadow">
      <div class="card-header">🧑‍💻 {{ event.community }}</div>
      <a href="{{ event.link }}">
        <img width="100%" style="aspect-ratio: 16/9" src="{{ site.baseurl }}/{{ event.localImg }}" />
        <div class="card-body">
          <h5 class="card-title">{{ event.title }}</h5>
          <p class="card-text">
          </p>
          <div class="d-flex justify-content-between align-items-center text-right">
            <small class="text-muted place">
            {%- if event.place != null and event.place != "" -%}
            🏠 {{ event.place }}<br>📍 {{ event.placeAddr }}
            {%- else −%}
            🌍 en ligne
            {%- endif -%}
            </small>
            <small class="text-muted text-end time">{{ event.dateFr }}📅<br>{{ event.timeFr }}⌚</small>
          </div>
        </div>
      </a>
    </div>
  </div>

{%- endfor -%}

  </div>
</div>

<!-- conférences -->
<section class="container my-2">

  <!-- title -->
  <div class="row">
    <div class="mx-auto text-center">
      <h1 class="fw-light">Conférences</h1>
      <p class="lead text-muted">
        Les grandes conférences tech annuelles sur Toulouse !
      </p>
    </div>
  </div>

  <div class="row row-cols-1 row-cols-md-2 row-cols-xxl-4">
    <!-- Cloud Toulouse -->
    <div class="my-2 col">    
      <div class="card shadow">
        <div class="card-header">📢 Cloud Toulouse</div>
        <a href="https://cloudtoulouse.com/" class="muted">
          <img width="100%" style="aspect-ratio: 16/9" src="{{ site.baseurl }}/confs-imgs/cloud-toulouse.jpg" />
        </a>
        <div class="card-body">
          <a href="https://cloudtoulouse.com/" class="muted">
            <h5 class="card-title">15 mai 2025</h5>
          </a>
          <div class="d-flex align-items-left">
            <a href="https://cloudtoulouse.com/" title="Site Officiel" class="mx-1 link-secondary"><i class="bi bi-globe"></i></a>
            <a href="https://www.linkedin.com/company/cloud-toulouse/" title="Page LinkedIn" class="mx-1 link-secondary"><i class="bi bi-linkedin"></i></a>
            <a href="https://bsky.app/profile/cloud-toulouse.bsky.social" title="Page Bluesky" class="mx-1 link-secondary"><i class="bi bi-stars"></i></a>
            <a href="https://x.com/cloud_toulouse" title="Page X / Twitter" class="mx-1 link-secondary"><i class="bi bi-twitter-x"></i></a>
          </div>
        </div>
      </div>
    </div>
    <!-- Agile Tour Toulouse -->
    <div class="my-2 col">
      <div class="card shadow">
        <div class="card-header">📢 Agile Tour Toulouse</div>
        <a href="https://tour.agiletoulouse.fr/"><img width="100%" style="aspect-ratio: 16/9" src="{{ site.baseurl }}/confs-imgs/agile-tour.jpg" /></a>
        <div class="card-body">
          <a href="https://tour.agiletoulouse.fr/" class="muted">
            <h5 class="card-title">12 et 13 juin 2025</h5>
          </a>
          <div class="d-flex align-items-left">
            <a href="https://tour.agiletoulouse.fr/" title="Site Officiel" class="mx-1 link-secondary"><i class="bi bi-globe"></i></a>
            <a href="https://www.linkedin.com/company/agile-toulouse" title="Page LinkedIn" class="mx-1 link-secondary"><i class="bi bi-linkedin"></i></a>
            <a href="https://twitter.com/agiletoulouse" title="Page X / Twitter" class="mx-1 link-secondary"><i class="bi bi-twitter-x"></i></a>
          </div>
        </div>
      </div>
    </div>
    <!-- DevFest Toulouse -->
    <div class="my-2 col">
      <div class="card shadow">
        <div class="card-header">📢 DevFest Toulouse</div>
        <a href="https://devfesttoulouse.fr/"><img width="100%" style="aspect-ratio: 16/9" src="{{ site.baseurl }}/confs-imgs/devfest.jpg" /></a>
        <div class="card-body">
          <a href="https://devfesttoulouse.fr/" class="muted">
            <h5 class="card-title">13 novembre 2025</h5>
          </a>
          <div class="d-flex align-items-left">
            <a href="https://devfesttoulouse.fr/" title="Site Officiel" class="mx-1 link-secondary"><i class="bi bi-globe"></i></a>
            <a href="https://www.linkedin.com/company/devfesttoulous" title="Page LinkedIn" class="mx-1 link-secondary"><i class="bi bi-linkedin"></i></a>
            <a href="https://bsky.app/profile/devfesttoulouse.fr" title="Page Bluesky" class="mx-1 link-secondary"><i class="bi bi-stars"></i></a>
            <a href="https://twitter.com/devfesttoulouse" title="Page X / Twitter" class="mx-1 link-secondary"><i class="bi bi-twitter-x"></i></a>
          </div>
        </div>
      </div>
    </div>
    <!-- Capitole du Libre -->
    <div class="my-2 col">
      <div class="card shadow">
        <div class="card-header">📢 Le Capitole du Libre</div>
        <a href="https://capitoledulibre.org/"><img width="100%" style="aspect-ratio: 16/9" src="{{ site.baseurl }}/confs-imgs/capitole-du-libre.jpg" /></a>
        <div class="card-body">
          <a href="https://capitoledulibre.org/" class="muted">
            <h5 class="card-title">15 et 16 novembre 2025</h5>
          </a>
          <div class="d-flex align-items-left">
            <a href="https://capitoledulibre.org/" title="Site Officiel" class="mx-1 link-secondary"><i class="bi bi-globe"></i></a>
            <a href="https://linkedin.com/company/capitole-du-libre" title="Page LinkedIn" class="mx-1 link-secondary"><i class="bi bi-linkedin"></i></a>
            <a href="https://framapiaf.org/@capitoledulibre" title="Page Mastodon" class="mx-1 link-secondary"><i class="bi bi-mastodon"></i></a>
            <a href="https://bsky.app/profile/capitoledulibre.org" title="Page Bluesky" class="mx-1 link-secondary"><i class="bi bi-stars"></i></a>
            <a href="https://x.com/capitoledulibre" title="Page X / Twitter" class="mx-1 link-secondary"><i class="bi bi-twitter-x"></i></a>
          </div>
        </div>
      </div>
    </div>
  </div>

  <!-- list automatique (obsolete) -->
  <!--
  <div class="row">
  {% assign months = "janvier|février|mars|avril|mai|juin|juillet|août|septembre|octobre|novembre|décembre" | split: "|" %}
  {% assign confs_by_date = site.data.confs | sort: "date" %}
  {% for conf in confs_by_date %}
  <div class="my-2 col-4">
    <div class="card shadow">
      <div class="card-header">📢 {{ conf.name }}</div>
      <a href="{{ conf.url }}">
        <div class="card-body">
          {% assign mi = conf.date | date: "%m" | minus: 1 %}
          {% assign month = months[mi] %}
          <h5 class="card-title">{{ conf.date | date: "%d" }} {{ month }} {{ conf.date | date: "%Y" }}</h5>
        </div>
      </a>
    </div>
  </div>
  {% endfor %}
  </div>
  -->

</section>

<!-- communautés -->
<section class="container my-2">

  <!-- title -->
  <div class="row ">
    <div class="mx-auto text-center">
      <h1 class="fw-light">Communautés</h1>
      <p class="lead text-muted">
        La liste des communautés tech toulousaines référencées.
      </p>
    </div>
  </div>

  <!-- list -->
  <div class="row row-cols-1 row-cols-md-2 row-cols-xxl-3">
  {% assign groups_by_name = site.data.groups | sort: "name" %}
  {% for group in groups_by_name %}
  <div class="my-2 col">
    <div class="card shadow">
    <div class="card-body">
      🧑‍💻 <a href="{{ group.url }}">{{ group.name }}</a>
    </div>
    </div>
  </div>
  {% endfor %}
  </div>

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
    <!-- <p class="mb-1">Album example is © Bootstrap, but please download and customize it for yourself!</p> -->
    <!-- <p class="mb-0">New to Bootstrap? <a href="/">Visit the homepage</a> or read our <a href="/docs/5.0/getting-started/introduction/">getting started guide</a>.</p> -->
  </div>
</footer>
