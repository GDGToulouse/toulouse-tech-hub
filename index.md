---
title: Agenda des communautés tech toulousaines
layout: default
---

<script type="text/javascript">
  function copyToClipboard(name) {
    var elt = document.getElementById(name);
    if(!elt) return;
    elt.select();
    elt.setSelectionRange(0, 99999); // for mobile devices??
    navigator.clipboard.writeText(elt.value);
    
    var tooltip = bootstrap.Tooltip.getOrCreateInstance(elt, { title: 'URL copiée !', trigger: 'manual'});
    tooltip.show();
    setTimeout(function() { tooltip.hide()}, 3000);
  }
</script>

<section class="py-5 text-center container">
    <div class="row py-lg-5">
      <div class="col-lg-6 col-md-8 mx-auto">
        <h1 class="fw-light">Agenda des communautés<br>tech toulousaines</h1>
        <p class="lead text-muted">
            Retrouvez tous les prochains évènements des communautés toulousaines sur une seule page.
        </p>
        <p>        
          <div class="input-group my-2">
            <span class="input-group-text"><i class="bi bi-calendar2-week"></i>&nbsp;iCal</span>
            <input id="icsInput" type="text" value="{{ site.site }}{{ site.baseurl }}/events.ics" readonly class="form-control" style="background-color:#fff" onfocus="this.select()">
            <button class="btn btn-outline-secondary" type="button" id="button-addon2" title="Copier l'URL dans le presse-papier" onclick="copyToClipboard('icsInput')"><i class="bi bi-clipboard-check"></i></button>
          </div>
          <div class="input-group my-2">
            <span class="input-group-text"><i class="bi bi-rss"></i>&nbsp;Rss/Atom</span>
            <input id="atomInput" type="text" value="{{ site.site }}{{ site.baseurl }}/events.atom.xml" readonly class="form-control" style="background-color:#fff" onfocus="this.select()">
            <button class="btn btn-outline-secondary" type="button" id="button-addon2" title="Copier l'URL dans le presse-papier" onclick="copyToClipboard('atomInput')"><i class="bi bi-clipboard-check"></i></button>
          </div>
          <div class="input-group my-2">
            <span class="input-group-text"><i class="bi bi-braces"></i>&nbsp;Json</span>
            <input id="jsonInput" type="text" value="{{ site.site }}{{ site.baseurl }}/events.json" readonly class="form-control" style="background-color:#fff" onfocus="this.select()">
            <button class="btn btn-outline-secondary" type="button" id="button-addon2" title="Copier l'URL dans le presse-papier" onclick="copyToClipboard('jsonInput')"><i class="bi bi-clipboard-check"></i></button>
          </div>
          <!--
          <a href="events.atom.xml" class="btn btn-secondary my-2">RSS/ATOM</a>
          <a href="events.json" class="btn btn-secondary my-2">Json</a>
          -->
        </p>
      </div>
    </div>  
</section>

<div class="container">
  <div class="row row-cols-1 row-cols-md-2 row-cols-xxl-3 g-3">

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
            <small class="text-muted">
            {%- if event.place -%}
            🏠 {{ event.place }}<br>📍 {{ event.placeAddr }}
            {%- endif -%}
            </small>
            <small class="text-muted text-end">{{ event.dateFr }}📅<br>{{ event.timeFr }}⌚</small>
          </div>
        </div>
      </a>
    </div>
  </div>

{%- endfor -%}

  </div>
</div>

<footer class="text-muted py-5">
  <div class="container">
    <p class="float-end mb-1">
      <a href="#">Retour en haut</a>
    </p>
    <p>Votre communauté n'est pas présente et vous souhaitez qu'elle soit ajoutée ?<br>Venez le signaler sur <a href="https://github.com/GDGToulouse/toulouse-tech-hub">github.com/GDGToulouse/toulouse-tech-hub</a>.</p>
    <!-- <p class="mb-1">Album example is © Bootstrap, but please download and customize it for yourself!</p> -->
    <!-- <p class="mb-0">New to Bootstrap? <a href="/">Visit the homepage</a> or read our <a href="/docs/5.0/getting-started/introduction/">getting started guide</a>.</p> -->
  </div>
</footer>